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
    })
    .AddCors(options =>
    {
        options.AddPolicy("customPolicy", policyBuilder =>
            policyBuilder.AllowAnyOrigin());
    });

// Adding YARP service
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

WebApplication app = builder.Build();
app.UseXfo(options => options.SameOrigin());
app.UseXContentTypeOptions();

await app.BootUmbracoAsync();

app.UseHttpsRedirection();

if (builder.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
else
    app.UseHsts();

app.UseRouting();
app.UseCors();
app.MapReverseProxy();

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