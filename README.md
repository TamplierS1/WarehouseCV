# WarehouseCV – Computer Vision Warehouse Management

WarehouseCV is a full‑stack web application for warehouse inventory tracking enhanced with computer vision. Users can manage products, create inventory sessions, and upload images of warehouse zones; a Python‑based AI model automatically counts products in the images and records the results as geotagged reports.

The app supports multiple users with JWT authentication, exports data to Excel, and runs entirely in Docker.

## Features

    User authentication – register & login with JWT tokens

    Product management – CRUD, bulk Excel import/export

    Inventory sessions – group reports by inventory runs

    Computer vision – automatic product counting from uploaded photos (YOLO + custom pipeline)

    Geospatial data – each report is pinned on a map (PostGIS geography column)

    Excel export – download aggregated product summary per inventory

    Dockerised – ready‑to‑run with PostgreSQL/PostGIS and Python

## Authentication

All endpoints except POST /api/accounts/login and POST /api/accounts/register require an Authorization: Bearer <token> header.
The token is obtained after login and stored in the browser’s localStorage.

## Computer Vision Pipeline

    User uploads a photo of a warehouse zone.

    The backend saves the image, then calls Scripts/pipe_counting.py.

    The Python script runs a YOLO model (final_weights.pt) to detect products.

    The count is returned and saved in the report’s ProductCount.

    The processed image is saved back (with bounding boxes, if configured).

## Docker Details

The Dockerfile uses a multi‑stage build:

    Build stage – compiles the .NET project.

    Runtime stage – based on ultralytics/ultralytics:latest-cpu, adds the .NET runtime, your custom Python packages, the EF migration tool, and the entrypoint script.

docker-compose.yml defines two services:

    db – PostgreSQL 16 + PostGIS

    backend – the ASP.NET app (auto‑runs EF migrations on startup)

Environment variables in the compose file override appsettings.json for database host, JWT keys, etc.

## Excel Features

Upload an Excel file with columns Mark, Name, Designation, Classifier, ClassifierGroup to mass‑import products.
Download the full product list as an Excel file.
Inside each inventory, export an aggregated summary showing how many of each product were detected across all reports.

## Users & Security

Every inventory, report, and product is scoped to the authenticated user (except products, which are shared).
Users can only view/modify their own inventories and reports.
JWT tokens expire after 1 hour.
