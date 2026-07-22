[CmdletBinding()]
param(
    [string]$ServerInstance = "localhost\SQLEXPRESS",
    [string]$Database = "TravelTime",
    [string]$Username = "admin",
    [string]$Email = ""
)

$securePassword = Read-Host "Enter the initial administrator password" -AsSecureString
$pointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword)

try {
    $password = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($pointer)
    if ([string]::IsNullOrWhiteSpace($password) -or $password.Length -lt 12) {
        throw "Use a password containing at least 12 characters."
    }

    $iterations = 600000
    $salt = New-Object byte[] 16
    $random = [Security.Cryptography.RandomNumberGenerator]::Create()
    try {
        $random.GetBytes($salt)
    }
    finally {
        $random.Dispose()
    }

    $derive = [Security.Cryptography.Rfc2898DeriveBytes]::new(
        $password,
        $salt,
        $iterations,
        [Security.Cryptography.HashAlgorithmName]::SHA256)
    try {
        $hash = $derive.GetBytes(32)
    }
    finally {
        $derive.Dispose()
    }

    $storedPassword = '$pbkdf2-sha256$' + $iterations + '$' +
        [Convert]::ToBase64String($salt) + '$' +
        [Convert]::ToBase64String($hash)

    $builder = New-Object Data.SqlClient.SqlConnectionStringBuilder
    $builder.DataSource = $ServerInstance
    $builder.InitialCatalog = $Database
    $builder.IntegratedSecurity = $true
    $builder.Encrypt = $true
    $builder.TrustServerCertificate = $true

    $connection = New-Object Data.SqlClient.SqlConnection($builder.ConnectionString)
    $command = $connection.CreateCommand()
    $command.CommandText = @"
IF EXISTS (SELECT 1 FROM dbo.Users WHERE Username = @Username)
    THROW 51000, 'That username already exists.', 1;

INSERT INTO dbo.Users (Username, UserPassword, RoleName, IsActive, Email)
VALUES (@Username, @Password, N'Admin', 1, NULLIF(@Email, N''));
"@
    [void]$command.Parameters.Add("@Username", [Data.SqlDbType]::NVarChar, 100)
    [void]$command.Parameters.Add("@Password", [Data.SqlDbType]::VarChar, 512)
    [void]$command.Parameters.Add("@Email", [Data.SqlDbType]::NVarChar, 255)
    $command.Parameters["@Username"].Value = $Username
    $command.Parameters["@Password"].Value = $storedPassword
    $command.Parameters["@Email"].Value = $Email

    try {
        $connection.Open()
        [void]$command.ExecuteNonQuery()
        Write-Host "Administrator '$Username' created."
    }
    finally {
        $command.Dispose()
        $connection.Dispose()
    }
}
finally {
    if ($pointer -ne [IntPtr]::Zero) {
        [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($pointer)
    }
    $password = $null
}
