$commands = @(
    "cd MeetingWebsite.Infrastracture",
    "dotnet ef database drop --force --context DataContext --startup-project ../MeetingWebsite.Web",
    "dotnet ef database update --context DataContext --startup-project ../MeetingWebsite.Web",
    "dotnet ef database update --context IdentityContext --startup-project ../MeetingWebsite.Web",
    "cd .."
)

foreach ($comand in $commands) {
    Invoke-Expression $comand    
}