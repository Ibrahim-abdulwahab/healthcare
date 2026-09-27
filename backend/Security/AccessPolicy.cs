namespace Healthcare.Api.Security;

public static class AccessPolicy
{
    public static bool Allows(string role, string resource, string method)
    {
        if (resource == "AuditLogs") return role == "admin" && method == "GET";
        if (role == "admin") return true;
        if (role is not ("doctor" or "staff")) return false;
        if (method == "DELETE") return role == "doctor" && resource is "Schedules" or "ScheduleExceptions";
        return resource switch
        {
            "Doctors" or "Locations" or "DoctorLocations" or "Reports" => method == "GET",
            "Patients" or "Appointments" or "PaymentRecords" => method == "GET" || role == "staff",
            "Visits" => method == "GET" || role == "staff" || method == "PUT",
            "Consultations" => method == "GET" || role == "doctor",
            "FollowUps" or "Prescriptions" or "PrescriptionItems" or "LabRequests" or "ImagingRequests" => method == "GET" || role == "doctor",
            "Schedules" or "ScheduleExceptions" => method == "GET" || role == "doctor",
            "CodeLists" or "Items" or "ItemCategories" or "VisitTypes" => method == "GET",
            "HealthcareFiles" => true, // Parent read/write policy is checked separately.
            "Notifications" => method == "GET" || method == "PATCH",
            _ => false
        };
    }
}
