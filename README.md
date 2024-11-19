# [`IfiNavet.Web.Core`](IfiNavet.Web.Core)

[`IfiNavet.Web.Core`](IfiNavet.Web.Core) is a web application built using Umbraco CMS. This project is designed to manage and deliver content efficiently using Umbraco's powerful content management features.

## Features ✨

-   Content management with Umbraco CMS
-   Azure Blob Storage integration for media files
-   Secure session management
-   Customizable logging with Serilog

## Usage 🚀

### Running the Application

To run the application, use the following command:

```sh
dotnet run --project IfiNavet.Web.Core

```

### Secure Session Management 🔒

The application ensures that the `UMB_SESSION` cookie is only stored when using HTTPS. This is configured in the [`Program.cs`](IfiNavet.Web.Core/Program.cs) file.

### Removing Excessive Headers 🛡️

The application removes excessive headers by configuring Kestrel in the [`Program.cs`](IfiNavet.Web.Core/Program.cs) file.

## Configuration ⚙️

### Development Settings

To configure the application for development, update the `appsettings.Development.json` file. This file contains settings specific to the development environment, such as connection strings, logging levels, and other configurations.

Make sure to adjust the settings according to your development environment needs.

## Project Structure 📂

-   **Views**: Contains the Razor views for rendering the UI.
-   **ViewModels**: Contains the view models used to pass data between the controllers and views.
-   **Services**: Contains the service classes that encapsulate business logic.
-   **Models**: Contains the data models used throughout the application.
-   **Helpers**: Contains helper classes and methods used across the application.
-   **Controllers**: Contains the controller classes that handle HTTP requests and responses.

## Contributing 🤝

We'd love your help to make this project even better! To contribute, please follow these steps:

1. Fork the repository.
2. Create a new branch (`git checkout -b feature-branch`).
3. Make your changes.
4. Commit your changes (`git commit -m 'Add new feature'`).
5. Push to the branch (`git push origin feature-branch`).
6. Open a pull request.

Please make sure your code follows our coding standards and includes appropriate tests.

## Reporting Bugs 🐛

If you find a bug, please let us know by creating an issue in the repository. Provide as much detail as you can, including steps to reproduce the bug, the expected behavior, and any relevant screenshots or logs.

## License 📄

This project is licensed under the GNU Affero General Public License. See the [`LICENSE`](LICENSE) file for more details.

For more information on the GNU AGPL, see <https://www.gnu.org/licenses/>.

## Contact 📧

For any inquiries, please contact the project maintainers.

---

This project is developed and maintained by Ifi-Navet.
