import posthog from "posthog-js";

posthog.init(process.env.POSTHOG_API_KEY, {
    api_host: process.env.POSTHOG_HOST_URL,
    person_profiles: 'identified_only'
});