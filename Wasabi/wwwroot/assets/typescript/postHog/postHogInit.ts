import posthog from "posthog-js";

posthog.init(process.env.POSTHOG_API_KEY, {
    api_host: "/ingest",
    ui_host: "https://eu.posthog.com",
    person_profiles: 'identified_only'
});