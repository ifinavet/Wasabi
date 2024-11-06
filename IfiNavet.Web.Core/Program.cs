using IfiNavet.Web.Core.Services.Events;
using IfiNavet.Web.Core.Services.JobListings;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddDeliveryApi()
    .AddComposers()
    .AddAzureBlobMediaFileSystem() // This configures the required services for Media
    .AddAzureBlobImageSharpCache() // This configures the required services for the Image Sharp cache
    .Build();

// Makes sure UMB_SESSION cookie only gets stored when using HTTPS
builder.Services
    .AddSession(options =>
    {
        options.Cookie.Name = "UMB_SESSION";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    })
    .AddTransient<IJobListingSearchService, JobListingSearchService>() // Adding the job filter and search
    .AddTransient<IEventSearchService, EventSearchService>(); // Adding events filter and search


WebApplication app = builder.Build();

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
        u.UseBackOfficeEndpoints();
        u.UseWebsiteEndpoints();
    });

await app.RunAsync();