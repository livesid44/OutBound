#!/bin/bash
# Publish script for Linux deployment
echo "=========================================="
echo "Publishing Siebel Lead Backup Application"
echo "=========================================="
echo ""
echo "Choose publish mode:"
echo "1. Self-contained (includes .NET runtime, larger size, no .NET installation required)"
echo "2. Framework-dependent (requires .NET 8.0 runtime installed, smaller size)"
echo ""
read -p "Enter choice (1 or 2): " choice

# Clean previous builds
echo ""
echo "Cleaning previous builds..."
dotnet clean --configuration Release

if [ "$choice" == "1" ]; then
    echo "Publishing self-contained application..."
    dotnet publish --configuration Release --output ./publish --self-contained true --runtime linux-x64 -p:PublishSingleFile=true
    echo ""
    echo "Self-contained publish completed!"
    echo "The application includes the .NET runtime and does not require .NET to be installed."
else
    echo "Publishing framework-dependent application..."
    dotnet publish --configuration Release --output ./publish --self-contained false --runtime linux-x64
    echo ""
    echo "Framework-dependent publish completed!"
    echo "IMPORTANT: .NET 8.0 Runtime must be installed on the target machine."
    echo "Download from: https://dotnet.microsoft.com/download/dotnet/8.0"
fi

echo ""
echo "=========================================="
echo "Output location: ./publish"
echo "=========================================="
echo ""
echo "To run the application:"
echo "1. Navigate to the publish folder"
echo "2. Update appsettings.json with your database connection"
echo "3. Run: ./SiebelLeadBackup.Console"
echo ""
