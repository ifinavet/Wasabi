const attendeeInfo = document.getElementById("scannedAttendee");
const attendeeMemberIdFromInput = (<HTMLInputElement>document.getElementById("AttendeeMemberId"));
const attendeeIdFromInput = (<HTMLInputElement>document.getElementById("AttendeeId"));
const {attendees, eventId} = window.backendData;

export const onScanSuccess = (decodedText: string, decodedResult: string) => {
    // handle the scanned code as you like, for example:
    console.log(`Code matched = ${decodedText}`, decodedResult);
    let parsedData = JSON.parse(decodedText);
    if (parseInt(parsedData["eventId"]) === Number(eventId)) {
        attendeeMemberIdFromInput.value = parsedData["attendeeMemberId"];
        attendeeIdFromInput.value = parsedData["attendeeId"];

        attendeeInfo.innerHTML = attendees.find(item => item.attendeeId === parsedData["attendeeId"])["fullName"];
    }
};

export const onScanFailure = (error: any) => {
};

(window as any).onScanSuccess = onScanSuccess;
(window as any).onScanFailure = onScanFailure;