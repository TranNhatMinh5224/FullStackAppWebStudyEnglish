# Test Payment API Script

## Test 1: Process Payment for Course
Invoke-RestMethod -Uri "http://localhost:5029/api/payment/process" -Method POST -ContentType "application/json" -Body (Get-Content "test_payment_process_course.json" -Raw)

## Test 2: Process Payment for Teacher Package  
Invoke-RestMethod -Uri "http://localhost:5029/api/payment/process" -Method POST -ContentType "application/json" -Body (Get-Content "test_payment_process_teacher_package.json" -Raw)

## Test 3: Confirm Payment
Invoke-RestMethod -Uri "http://localhost:5029/api/payment/confirm" -Method POST -ContentType "application/json" -Body (Get-Content "test_payment_confirm.json" -Raw)