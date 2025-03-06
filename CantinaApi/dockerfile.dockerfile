# Use the official SQL Server image
FROM mcr.microsoft.com/mssql/server:2022-latest

# Set environment variables for SQL Server
ENV SA_PASSWORD="YourStrong!Passw0rd"
ENV ACCEPT_EULA="Y"

# Expose port 1433
EXPOSE 1433

# Run SQL Server when the container starts
CMD ["/opt/mssql/bin/sqlservr"]
