const attendeeMemberIdFromInput = (<HTMLInputElement>document.getElementById("AttendeeMemberId"));
const attendeeIdFromInput = (<HTMLInputElement>document.getElementById("AttendeeId"));
const shownUp = (<HTMLInputElement>document.getElementById("ShownUp"));

export const setAttendeeInForm = (attendeeMemberId: string, attendeeId: string) => {
    attendeeMemberIdFromInput.value = attendeeMemberId;
    attendeeIdFromInput.value = attendeeId;
    console.log(attendeeId, attendeeMemberId)
}

export const submitForm = (status: string) => {
    shownUp.value = status;

    (<HTMLFormElement>document.getElementById("register-attendee-form")).submit();
}

export const registerAttendeeFromDropdown = (attendeeId: string, attendeeMemberId: string, status: string) => {
    console.log(attendeeId);
    setAttendeeInForm(attendeeMemberId, attendeeId);
    submitForm(status)
}

(window as any).submitForm = submitForm;
(window as any).setAttendeeInForm = setAttendeeInForm;
(window as any).registerAttendeeFromDropdown = registerAttendeeFromDropdown;
