[Reflection.Assembly]::LoadWithPartialName("System.Net.Http") | Out-Null

# DeaconCare Cryptographic SMS Validation Probe
Write-Host "[Test Trigger] Initializing simulated cellular data transmission..." -ForegroundColor Cyan

# 1. Configuration variables matching our local Kestrel server parameters
$TargetUrl = "http://localhost:5000"
$AuthToken = "mock_twilio_auth_token_secret_placeholder"

# 2. Formulate the raw incoming text message parameters
$BodyField = "CLAIM 402"
$FromField = "+12025992161"

# 3. Cryptographic Signature Generation (Explicitly Aligned to TwilioSecurityMiddleware)
$SignatureData = $TargetUrl + "Body" + $BodyField + "From" + $FromField

$Hmac = New-Object System.Security.Cryptography.HMACSHA1
$Hmac.Key = [System.Text.Encoding]::UTF8.GetBytes($AuthToken)
$HashBytes = $Hmac.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($SignatureData))
$ComputedSignature = [Convert]::ToBase64String($HashBytes)

Write-Host "Sending encrypted payload over HTTPS TLS 1.3 to $TargetUrl..." -ForegroundColor Yellow

# Native .NET HttpClient execution track
$Handler = New-Object System.Net.Http.HttpClientHandler
$Client = New-Object System.Net.Http.HttpClient($Handler)

# Format the parameters exactly like an industry-standard cellular network wave form
$RawPayloadText = "Body=CLAIM+402&From=%2B12025992161"
$RawBytes = [System.Text.Encoding]::ASCII.GetBytes($RawPayloadText)

$Content = New-Object System.Net.Http.ByteArrayContent($RawBytes, 0, $RawBytes.Length)
$Content.Headers.ContentType = New-Object System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded")

$Client.DefaultRequestHeaders.Add("X-Twilio-Signature", $ComputedSignature)

try {
    $PostTask = $Client.PostAsync($TargetUrl, $Content)
    $Response = $PostTask.Result
    
    $ReadTask = $Response.Content.ReadAsStringAsync()
    $ServerContent = $ReadTask.Result
    
    if ($Response.IsSuccessStatusCode) {
        Write-Host ""
        Write-Host "[Server Response Received]:" -ForegroundColor Green
        Write-Host $ServerContent -ForegroundColor Green
    } else {
        Write-Host ""
        Write-Host "[Server Security Block Intercepted]:" -ForegroundColor Red
        Write-Host "The firewall rejected the token signature payload: $ServerContent" -ForegroundColor Red
    }
} catch {
    Write-Host ""
    Write-Host "Pipeline Connection Failed: $_" -ForegroundColor Red
} finally {
    if ($null -ne $Client) { $Client.Dispose() }
}