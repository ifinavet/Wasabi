# Wasabi

[`Wasabi`](Wasabi) Is the codebase for powering the website <ifinavet.no> and it is built on asp.net 8,
using the CMS Umbraco.

> [!WARNING]  
> This project is not in production yet, and under active development.
> If you want to contribute please contact Webansvarlig at IFI-Navet via email at <web@ifinavet.no>.

## Usage 🚀

### Running the Application

To run the application, use the following command:

```sh
cd Wasabi
dotnet build
dotnet run
```

Alternatively:

```sh
dotnet run --project Wasabi
```

### Testing

_**Coming soon.**_

## Configuration ⚙️

### Development Settings

To configure the application for development, update the `appsettings.json` file. This file contains settings specific
to the development environment, such as connection strings, logging levels, and other configurations.

Environment variables are marked with the notion `<ENVIROMENTVARIABLE_HERE>`, and can be replaced by the .NET Secrets
manager like this:

```sh
dotnet user-secrets set "Umbraco:CMS:Global:Smtp:From": "hei@test.no" --project "PATH_TO_PROJECT\Wasabi"
```

For more info about secrets in .net visit the
[Microsoft documentation](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-9.0&tabs=windows).

Make sure to adjust the settings according to your development environment needs.

## Project Structure 📂

- [`Views`](Wasabi/Views): Contains the Razor views for rendering the UI.
- [`ViewModels`](Wasabi/ViewModels): Contains the view models used to pass data between the controllers and views.
- [`Services`](Wasabi/Services): Contains the service classes that encapsulate business logic.
- [`Models`](Wasabi/Models): Contains the data models used throughout the application.
- [`Helpers`](Wasabi/Helpers): Contains helper classes and methods used across the application.
- [`Controllers`](Wasabi/Controllers): Contains the controller classes that handle HTTP requests and
  responses.

## Contributing 🤝

We'd love your help to make this project even better! To contribute, please follow these steps:

1. Clone the repository.
2. Create a new branch (`git checkout -b feature-branch`).
3. Make your changes.
4. Commit your changes (`git commit -m 'Add new feature'`).
5. Push to the branch (`git push origin feature-branch`).
6. Open a pull request.

Please make sure your code follows our coding standards and includes appropriate tests.

### Umbraco documentation 🔧

For documentation about the CMS visit [docs.umbraco.com](https://docs.umbraco.com/umbraco-cms/13.latest)

## Reporting Bugs 🐛

If you find a bug, please let us know by creating an issue in the repository. We would love to check it out and find a
solution. Provide as much detail as you can (the more the merrier), including steps to reproduce the bug, the expected
behavior, and any relevant screenshots or logs.

## License 📄

This project is licensed under the GNU Affero General Public License. See the [`LICENSE`](LICENSE) file for more
details.

## Contact 📧

For any inquiries, please contact us.

E-mail: <web@ifinavet.no>

---

This project is developed and maintained by Ifi-Navet at the University of Oslo, Department of Informatics.
