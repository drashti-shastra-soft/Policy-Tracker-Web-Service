# Policy Tracker – setup

Two solutions, same layout as AttendEdge:

- **Service** – `E:\AE_WS\Service\Repos\PolicyTracker\PolicyTrackerWebService.sln` (Web API, https://localhost:44391)
- **Client** – `E:\AE_WS\Client\Repos\PolicyTracker\PolicyTrackerWebClient.sln` (MVC, https://localhost:44392)

## 1. Database
1. Open `Database\PolicyTrackerDB.sql` in SSMS and run it (F5). It creates **PolicyTrackerDB** and loads
   163 policies, 109 broker-statement lines (Mar–Sep 2026, total ₹2,42,919) and the 8 bank credits.
2. In the service `Web.config`, set the `PolicyTrackerDBEntities` connection string to your SQL Server
   (default: `.\SQLEXPRESS` with Windows authentication).

## 2. Build and run
1. Open each solution in Visual Studio 2022 → right-click the solution → **Restore NuGet Packages** → Build.
2. Start the service first, then the client (or set both as startup projects in one VS window each).
3. If you change the service port, update `APIBaseUrl` in the client `Web.config` (it must end in `/api`).

## 3. First login
- Email: **admin@policytracker.local**
- Password: **NQjkL8h1lrU9V@7**
- Change it straight away (sidebar → Change Password), then add your staff under **Users**.

## 4. Before going live
- Replace `SecretKey` in the service `Web.config` with your own random value (32+ characters).
- Fill the **Points Grid** (Masters) from the broker's payout grid so expected commission uses age bands;
  until then it uses the points written on each policy, else the category default.
