

function ClearUserLoggedInTable() {
    try {
        const serverName = location.host; // gets the server name
        const connectionString = `Server=${serverName};Database=GroupManagementDB;Trusted_Connection=True;MultipleActiveResultSets=true;`;//code to connect to the database
        const request = new XMLHttpRequest();
        request.open("POST", "/Users/ClearUserLoggedInTable");
        request.send();
    } catch (error) {
        console.log(error);
    }
}