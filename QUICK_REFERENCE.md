# Quick Reference - New Features

## New Navigation Buttons (Advanced Features Section)

### 📜 Certifications
**Purpose:** Track staff certifications and ensure compliance
**Key Actions:**
- View all certifications with expiry alerts
- Add new certifications with upload support
- Filter expired/expiring certifications
- Update verification status

**Critical Features:**
- ⚠ Red alert for expired certifications
- ⚠ Orange alert for expiring within 30 days
- Document upload capability
- Staff-linked tracking

---

### 📦 Inventory Management
**Purpose:** Monitor supplies and prevent stockouts
**Key Actions:**
- Add/edit inventory items
- Link items to suppliers
- Set minimum stock levels
- Track quantities and pricing

**Critical Features:**
- ⚠ Automatic low stock warnings
- Category-based organization
- Supplier integration
- Cost tracking per unit

---

### 📆 Staff Scheduling
**Purpose:** Manage staff shifts and assignments
**Tabs:**
1. **Shifts** - Create shift templates (e.g., "Morning 8-16")
2. **Staff Assignments** - Assign staff to shifts on specific dates

**Key Actions:**
- Define shift times
- Assign staff to shifts
- View daily schedule
- Filter by date

---

### 📊 Reports & Analytics
**Purpose:** Business intelligence and operational insights
**4 Comprehensive Tabs:**

#### 1. Revenue Report
- Select date range
- View bookings vs concession revenue
- See daily breakdown
- Total revenue calculation

#### 2. Occupancy Tracking
- Record real-time visitor counts
- Calculate capacity percentages
- Monitor area saturation
- Status: OK (green), BUSY (orange), FULL (red)

#### 3. Loyalty & Memberships
- View active memberships
- Track loyalty points by tier
- Monitor points earned vs redeemed
- Customer engagement metrics

#### 4. Weather Conditions
- Record weather by area
- Log temperature and conditions
- Set operational status
- Historical weather tracking

---

## Database Enhancements

### New Entity Classes (All following existing patterns)
✅ Certification - Staff certifications tracking
✅ Shift - Shift time templates  
✅ StaffShift - Staff-shift assignments
✅ Inventory - Stock items
✅ Supplier - Vendor management
✅ Concession - Food/retail stands
✅ ConcessionSale - Sales transactions
✅ MedicalInfo - Visitor health info
✅ Membership - Membership programs
✅ LoyaltyPoints - Points system
✅ OccupancyTracking - Capacity monitoring
✅ SafetyStandard - Compliance standards
✅ AttractionSafetyStandard - Attraction compliance
✅ MaintenancePart - Replacement parts
✅ MaintenancePartCompat - Part compatibility
✅ PriceListItem - Ticket pricing
✅ PriceListRule - Pricing rules link
✅ PricingRule - Dynamic pricing rules
✅ Training - Staff training records
✅ UserRole - User-role assignments
✅ Waiver - Legal waivers
✅ WeatherCondition - Weather logs

### Updated Entities
✅ Visitor - Now includes MedicalInfoId reference
✅ Payment - Enhanced with refund tracking

---

## Common UI Patterns

### All Windows Include:
✅ Search/filter functionality
✅ DataGrid with sortable columns
✅ Add/Update/Delete buttons
✅ Form validation
✅ Status messages
✅ Clear form functionality
✅ Enable/disable buttons based on selection

### Color Coding:
- 🟢 Green = Good/OK/Active
- 🟠 Orange = Warning/Expiring/Busy
- 🔴 Red = Critical/Expired/Full

---

## Workflow Examples

### Daily Operations
1. **Morning:**
   - Check Certifications for expired/expiring
   - Review Staff Scheduling for today
   - Check Inventory for low stock
   
2. **Throughout Day:**
   - Record Occupancy at peak times
   - Update Weather Conditions
   - Process Bookings (existing)

3. **End of Day:**
   - Generate Revenue Report
   - Review Occupancy patterns
   - Update Inventory after sales

### Weekly Tasks
- Review all certifications
- Plan next week's staff schedule
- Order inventory items below minimum
- Analyze revenue trends

### Monthly Tasks
- Generate comprehensive revenue reports
- Review membership renewals
- Update loyalty tiers
- Analyze occupancy patterns
- Plan maintenance based on weather

---

## Tips & Best Practices

### Certifications
- Upload PDF copies of certificates
- Set reminders 45 days before expiry
- Maintain verification status

### Inventory
- Set minimum stock to 10-15% of average usage
- Review supplier performance
- Update unit prices regularly

### Scheduling
- Create standard shift templates first
- Assign shifts week in advance
- Check for conflicts

### Reports
- Generate revenue reports monthly
- Record occupancy during peak hours
- Update weather conditions daily
- Review loyalty tiers quarterly

---

## Support & Maintenance

### Connection String
Located in: `AquaParkContext.cs`
```csharp
optionsBuilder.UseOracle("User Id=st72504;Password=...;Data Source=...")
```

### Common Issues
- **Can't add record:** Check required fields (marked with *)
- **Update button disabled:** Select a record first
- **Filter not working:** Clear search box and try again

### Data Integrity
- All entities have audit trails (Created/Updated By/At)
- Foreign keys enforce referential integrity
- Validation prevents invalid data

---

**Last Updated:** December 11, 2025
**Version:** 2.0 - Advanced Features
