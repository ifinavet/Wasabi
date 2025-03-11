const attendees: { [key: string]: any }[] = window.backendData.attendees;

const copyToClipboard = (text: string, success_message: string) => {
    if (navigator.clipboard && navigator.clipboard.writeText) {
        navigator.clipboard
            .writeText(text)
            .then(() => alert(success_message))
            .catch((err) => {
                console.error("Failed to copy text: ", err);
            });
    } else {
        // Fallback for older browsers
        const textArea = document.createElement("textarea");
        textArea.value = text;

        // Make the textarea out of viewport
        textArea.style.position = "fixed";
        textArea.style.left = "-999999px";
        textArea.style.top = "-999999px";
        document.body.appendChild(textArea);

        textArea.focus();
        textArea.select();

        try {
            const successful = document.execCommand("copy");
            alert(success_message);
        } catch (err) {
            console.error("Fallback: Oops, unable to copy", err);
        }

        document.body.removeChild(textArea);
    }
}

export const generateColumns = (column_names: string[], success_message: string) => {
    let copy_text = "";

    attendees.forEach(attendee => {
        let row_text = "";
        column_names.forEach(column_name => {
            if (attendee[column_name] !== "" && attendee[column_name].length !== 0) {
                row_text += attendee[column_name] + '\t';
            }
        });
        if (row_text.trim().length > 0) {
            copy_text += row_text + '\n';
        }
    });

    copyToClipboard(copy_text, success_message);
}

(window as any).generateColumns = generateColumns;
(window as any).copyToClipboard = copyToClipboard;