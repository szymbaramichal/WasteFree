# WasteFree
WasteFree Cloud is a cloud-based web application for waste management. It helps organize, monitor, and optimize waste collection and disposal processes efficiently.

## Test users credentials

| Username | Password  |
|----------|-----------|
| test1    | Kwakwa5!  |
| test2    | Kwakwa5!  |
| test3    | Kwakwa5!  |
| test4    | Kwakwa5!  |
| test5    | Kwakwa5!  |

## Local Development with Docker Compose

To run the entire application locally using Docker Compose:

1. **Ensure Docker and Docker Compose are installed** on your machine.

2. **Clone the repository:**
   ```sh
   git clone https://github.com/szymbaramichal/WasteFree.git
   cd WasteFree
   ```

3. **Build and start the containers:**
   ```sh
   docker-compose up --build
   ```
   This will build and start all services defined in `docker-compose.yml`.

4. **Access the application:**
   - The API will be available at `http://localhost:8080` (use `http://localhost:8080/scalar/v1` to get api docs).
   - The UI will be available at the port specified in `docker-compose.yml`

### Environment-specific configuration
- The API uses `appsettings.Docker.json` for configuration when running in Docker (set by `ASPNETCORE_ENVIRONMENT=Docker`).
- You can modify `docker-compose.yml` to change environment variables, ports, or volumes as needed.

### Useful commands
- **Stop containers:**
  ```sh
  docker-compose down
  ```
- **View logs:**
  ```sh
  docker-compose logs
  ```