# PowerShell script to create placeholder PNG icons
# Run this to create minimal valid PNG files for testing

$icons = @(
    @{ Name = "ai_assistant_16.png"; Size = 16 },
    @{ Name = "ai_assistant_32.png"; Size = 32 },
    @{ Name = "tasks_16.png"; Size = 16 },
    @{ Name = "tasks_32.png"; Size = 32 },
    @{ Name = "default.png"; Size = 32 }
)

Add-Type -AssemblyName System.Drawing

foreach ($icon in $icons) {
    $bitmap = New-Object System.Drawing.Bitmap $icon.Size, $icon.Size
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    
    # Fill with company color
    $brush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(0, 111, 151))
    $graphics.FillRectangle($brush, 0, 0, $icon.Size, $icon.Size)
    
    # Add simple "AI" text for AI assistant icons
    if ($icon.Name -like "ai_assistant*") {
        $font = New-Object System.Drawing.Font("Arial", [Math]::Max(6, $icon.Size / 4), [System.Drawing.FontStyle]::Bold)
        $textBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::White)
        $graphics.DrawString("AI", $font, $textBrush, 1, 1)
        $font.Dispose()
        $textBrush.Dispose()
    }
    # Add simple "T" text for tasks icons
    elseif ($icon.Name -like "tasks*") {
        $font = New-Object System.Drawing.Font("Arial", [Math]::Max(6, $icon.Size / 3), [System.Drawing.FontStyle]::Bold)
        $textBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::White)
        $graphics.DrawString("T", $font, $textBrush, [Math]::Max(2, $icon.Size / 6), 1)
        $font.Dispose()
        $textBrush.Dispose()
    }
    
    $graphics.Dispose()
    $brush.Dispose()
    
    $bitmap.Save("$PSScriptRoot\$($icon.Name)", [System.Drawing.Imaging.ImageFormat]::Png)
    $bitmap.Dispose()
    
    Write-Host "Created $($icon.Name)" -ForegroundColor Green
}

Write-Host "`nPlaceholder icons created successfully!" -ForegroundColor Cyan