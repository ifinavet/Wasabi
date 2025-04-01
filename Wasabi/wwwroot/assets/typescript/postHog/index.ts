import posthog from "posthog-js";
declare var POSTHOG_PROJECT_API_KEY: string | undefined;

posthog.init(POSTHOG_PROJECT_API_KEY, {
    api_host: '/ingest',
    ui_host: "https://eu.posthog.com",
});
