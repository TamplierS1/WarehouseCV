using Python.Runtime;

namespace WarehouseCV.Services;

public class PythonScriptService : IDisposable
{
    private dynamic _pipeCounterModule;

    public PythonScriptService(string pythonDllPath, IWebHostEnvironment env)
    {
        if (!string.IsNullOrWhiteSpace(pythonDllPath))
        {
            Runtime.PythonDLL = pythonDllPath;
        }

        PythonEngine.Initialize();
        PythonEngine.BeginAllowThreads(); // release GIL for multithreaded ASP.NET

        using (Py.GIL())
        {
            dynamic sys = Py.Import("sys");

            // Add script to Python module search path
            sys.path.append(Path.Combine(env.ContentRootPath, "Scripts"));

            // Add model weights path to command line arguments the scritp accepts
            sys.argv = new PyList([
                new PyString("pipe_counting.py"),
                new PyString(Path.Combine(env.ContentRootPath, "Scripts/final_weights.pt")),
            ]);

            _pipeCounterModule = Py.Import("pipe_counting");
        }
    }

    public (byte[] imageBytes, int pipeCount) ProcessImage(
        string imagePath,
        int pipeType,
        string encodingFormat
    )
    {
        using (Py.GIL())
        {
            dynamic result = _pipeCounterModule.process_image(imagePath, pipeType);
            // result = [annotated numpy array, int count]

            PyObject imgArray = result[0];
            int count = result[1].As<int>();

            dynamic bytes = _pipeCounterModule.encode_image(imgArray, encodingFormat);
            byte[] jpgBytes = bytes.As<byte[]>();

            return (jpgBytes, count);
        }
    }

    public void Dispose()
    {
        PythonEngine.Shutdown();
    }
}
