# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY WarehouseCV/WarehouseCV.csproj WarehouseCV/
RUN dotnet restore WarehouseCV/WarehouseCV.csproj

COPY . .
RUN dotnet publish WarehouseCV/WarehouseCV.csproj -c Release -o /app/publish

# ---------- Runtime stage ----------
FROM ultralytics/ultralytics:latest-cpu AS runtime
USER root

# Install only the system packages needed by .NET runtime + EF migration check
RUN apt-get update && \
    apt-get install -y curl postgresql-client && \
    rm -rf /var/lib/apt/lists/*

# Install .NET runtime 
RUN curl -sSL https://dot.net/v1/dotnet-install.sh -o dotnet-install.sh && \
    chmod +x dotnet-install.sh && \
    ./dotnet-install.sh --channel 10.0 --runtime aspnetcore --install-dir /usr/share/dotnet && \
    rm dotnet-install.sh

ENV PATH="/usr/share/dotnet:${PATH}"
ENV DOTNET_ROOT="/usr/share/dotnet"

# Install Python dependencies
RUN pip install --no-cache-dir --break-system-packages opencv-python-headless numpy && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/* && \
    rm -rf /root/.cache/pip

# Copy Python scripts used by the app
COPY WarehouseCV/Scripts/ /app/Scripts/

# Copy the published app
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 5041
ENV ASPNETCORE_URLS=http://+:5041

ENTRYPOINT ["dotnet", "WarehouseCV.dll"]