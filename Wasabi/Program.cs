WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddDeliveryApi()
    .AddComposers()
    .AddAzureBlobMediaFileSystem() // This configures the required services for Media
    .AddAzureBlobImageSharpCache() // This configures the required services for the Image Sharp cache
    .Build();

// Removing Excessive headers
builder.WebHost.UseKestrel(options => options.AddServerHeader = false);

// Makes sure UMB_SESSION cookie only gets stored when using HTTPS
builder.Services
    .AddSession(options =>
    {
        options.Cookie.Name = "UMB_SESSION";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });

/*
#if DEVELOPMENT
builder.Services.AddSassCompiler();
#endif
*/

WebApplication app = builder.Build();
app.UseXfo(options => options.SameOrigin());
app.UseXContentTypeOptions();

await app.BootUmbracoAsync();

app.UseHttpsRedirection();

app.UseUmbraco()
    .WithMiddleware(u =>
    {
        u.UseBackOffice();
        u.UseWebsite();
    })
    .WithEndpoints(u =>
    {
        u.UseInstallerEndpoints();
        u.UseBackOfficeEndpoints();
        u.UseWebsiteEndpoints();
    });

await app.RunAsync();