import posthog from "posthog-js";

const consentModal = document.getElementById('ConsentModal');

document.addEventListener("DOMContentLoaded", () => {
    if (!localStorage.getItem("ConsentModal")) {
        consentModal.classList.add('active');  
    } else {
        consentModal.classList.add('hidden');
    }
})

export const handleConsent = (answer: boolean) => {
    if (answer) {
        posthog.set_config({persistence: "localStorage+cookie"})
    } else {
        posthog.opt_out_capturing()
        posthog.set_config({persistence: "memory"})
    }

    localStorage.setItem('ConsentModal', JSON.stringify(answer))
    consentModal.classList.add('hidden');
}

(window as any).handleConsent = handleConsent;