# Evently
An events-browsing platform built using ASP.NET Core Razor Pages, allowing users to host, discover and register (among other things) for upcoming events<br>
This project is **not** production ready and could do with some optimisations (more so regarding database reads).

<details open>
<summary><h2>Tech Stack and Tools</h2></summary>
<div style='margin-left: 12px'>

### Tech Stack
* Programming Languages: C# and TypeScript(*frontend scripts*)
* Framework: ASP.NET Core Razor Pages
* Data Access: Entity Framework Core and SQLite
* User Authentication: ASP.NET Core Identity
* External Email Service: PostMark

### Development Tools
* Version Control: Git
* Code Editor/IDE: JetBrains Rider
* UX/UI Mockups: Adobe XD
</div>
</details>


<details open>
<summary><h2>Purpose</h2></summary>

**With this project, I aimed to gain hands-on experience with building real-world applications, focusing on:**
* Designing and implementing a data model with Entity Framework Core.
* Implementing user authentication and authorization with ASP.NET Core Identity.
* Utilizing Razor Pages to create dynamic and interactive user interfaces.
* Integrating an external email service (PostMark) for user notifications.

**This project also served as a platform to explore and address challenges associated with:**
* Reading and writing to a SQL DB using *Entity Framework Core* for data access, including mapping existing C# classes to database tables.
* Implementing secure user authentication practices (such as email verification).
* Integrating external services like PostMark in order to send automated emails to users.
</details>


<details open>
<summary><h2>Screenshots</h2></summary>
    <div style="display: flex; justify-content: space-between;">
        <img src="/screenshots/evently-home-page.png" alt="Home Page" style="width:32%">
        <img src="/screenshots/evently-contact-us-page.png" alt="Contact Us Page" style="width:32%">
        <img src="/screenshots/evently-login-page.png" alt="Login Page" style="width:32%">
    </div>
    <div style="display: flex; justify-content: space-between;"><img src="/screenshots/evently-admin-page.png" alt="Admin Page"></div>
    <img src="/screenshots/evently-event-page.png" alt="Event Page" width="750">
</details>


<details open>
<summary><h2>Future Considerations</h2></summary>

Due to time constraints during development, I decided to prioritise functionality over efficiency in some areas.<br>

**In the future,** I'd begin by swapping some LINQ-based database reads with literal SQL queries, adding indexes for more commonly queried attributes, and potentially using SQL views for common queries (most popular events). I'd also work on further abstracting some functionality, resulting in a more efficient and cleaner codebase.
</details>


<details open>
<summary><h2>Emails via PostMark</h2></summary>

To increase account security, there are multiple points during the application where users are sent email notifications:
- Password resets
- Email changes
- Account email verification

This functionality utilises PostMark. To configure PostMark for use within this project, create a new PostMark server. From here, you'll find your *API Token(s)*; you'll need this later. 
</details>


<details open>
<summary><h2>Setup Instructions</h2></summary>

1. **Clone the Repository**:
    ```sh
    git clone https://github.com/JoshA02/Evently.git
    ```
2. **Install Dependencies**:
    ```sh
    cd Evently
    npm i
    ```
3. **DotNet User-Secrets**:
    This project uses Postmark to send emails to registered users. For this, both the 'from email'     and Postmark API token are grabbed from **DotNet *user secrets* file**. **To configure this:**
    1. Navigate to the project directory (`cd Evently`)
    2. Run `dotnet user-secrets init`
    3. Define both `PostmarkToken` and `PostmarkFromEmail`
        1. Navigate to the project directory (`cd Evently`)
        2. Set your PostMark API token by running `dotnet user-secrets set PostmarkToken [token]` from the project directory.
            - `[token]` can be found in your PostMark server's *'API Tokens'* tab.
        3. Set the email address PostMark will send emails from by running `dotnet user-secrets set PostmarkFromEmail [email]` from the project directory.
            - `[email]` should be the email address you used when signing up for PostMark.
4. **Compile TS to JS using tsc**:
    1. Navigate to the `Evently/evently/wwwroot/scripts` folder (`cd Evently/evently/wwwroot/scripts`)
    2. Run tsc to compile all TypeScript code in the scripts directory to JavaScript (`npx tsc`)
5. **Restore NuGet packages**:
    To reduce the size of the repo, I have omitted the `evently/obj` folder. Doing this means that you **must restore all NuGet packages** for this project before running.
    One way to do this is via the NuGet CLI:
    1. Navigate to the project directory (`cd Evently`)
    2. Run `nuget restore evently.sln`
6. **Add EFC Migration and Update Database**:
    The provided files do not include any prior database migrations. Due to this, you will need to add a new migration, before updating the database. To do this in *JetBrains Rider*:
   1. Navigate to `Tools > Entity Framework Core > Add Migration`, set `DbContext class` to `EventAppDbContext`, and click `OK`.
   2. Upon adding the migration, navigate to `Tools > Entity Framework Core > Update Database`, set `DbContext class` to `EventAppDbContext`, tick `Use the default connection of the startup project`, and click `OK`.
7. **Run the Application**:
    - You can run the app using Rider/Visual Studio's run buttons (select the 'http' launch profile).
    - Alternatively, you can manually run the `dotnet run --launch-profile "http"` command and visit `http://localhost:5139` in a web browser.

</details>


<details open>
<summary><h2>Known Bugs</h2></summary>

1. Navigating to the 'events' page with 0 accounts in the database results in an `InvalidOperationException`.
    - As a workaround, first create at least one user account.
</details>
