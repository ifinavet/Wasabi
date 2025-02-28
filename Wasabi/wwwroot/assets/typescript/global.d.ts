interface attendee {
    allergies: string,
    attendeeId: string,
    attendeeMemberId: string,
    email: string,
    fullName: string,
    preferredLanguage: string,
    username: string
}

interface Window {
    backendData: {
        eventId: string,
        attendees: attendee[]
    }
}