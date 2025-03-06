import {Html5QrcodeResult, Html5QrcodeScanner} from "html5-qrcode";

const attendeeInfo = document.getElementById("scannedAttendee");
const attendeeMemberIdFromInput = (<HTMLInputElement>document.getElementById("AttendeeMemberId"));
const attendeeIdFromInput = (<HTMLInputElement>document.getElementById("AttendeeId"));
const {attendees, eventId} = window.backendData;

export const onScanSuccess = (decodedText: string, decodedResult: Html5QrcodeResult) => {
    // handle the scanned code as you like, for example:
    console.log(`Code matched = ${decodedText}`, decodedResult);
    let parsedData = JSON.parse(decodedText);
    if (parseInt(parsedData["eventId"]) === Number(eventId)) {
        attendeeMemberIdFromInput.value = parsedData["attendeeMemberId"];
        attendeeIdFromInput.value = parsedData["attendeeId"];

        attendeeInfo.innerHTML = attendees.find(item => item.attendeeId === parsedData["attendeeId"])["fullName"];
    }
};


export let html5QrcodeScanner = new Html5QrcodeScanner(
    "reader",
    {fps: 10, qrbox: {width: 250, height: 250}},
    /* verbose= */ false);
html5QrcodeScanner.render(onScanSuccess, () => {
});

(window as any).onScanSuccess = onScanSuccess;
(window as any).html5QrcodeScanner = html5QrcodeScanner;