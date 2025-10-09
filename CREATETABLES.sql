CREATE TABLE staff (
    staff_id      NUMBER PRIMARY KEY,
    first_name    VARCHAR2(100) NOT NULL,
    last_name     VARCHAR2(100) NOT NULL,
    email         VARCHAR2(200) UNIQUE,
    phone         VARCHAR2(30),
    hire_date     DATE DEFAULT SYSDATE NOT NULL,
    active        CHAR(1) DEFAULT 'Y' CHECK (active IN ('Y','N')),
    job_title     VARCHAR2(100),
    notes         CLOB
);

-- Role master table
CREATE TABLE roles (
    role_id NUMBER PRIMARY KEY,
    role_name VARCHAR2(100) NOT NULL UNIQUE,
    description VARCHAR2(4000)
);

-- Many-to-many staff_roles (a staff can have many roles)
CREATE TABLE staff_roles (
    staff_role_id NUMBER PRIMARY KEY,
    staff_id      NUMBER NOT NULL,
    role_id       NUMBER NOT NULL,
    assigned_date DATE DEFAULT SYSDATE,
    CONSTRAINT fk_sr_staff FOREIGN KEY (staff_id) REFERENCES staff(staff_id) ON DELETE CASCADE,
    CONSTRAINT fk_sr_role  FOREIGN KEY (role_id)  REFERENCES roles(role_id)  ON DELETE CASCADE,
    CONSTRAINT u_staff_role UNIQUE (staff_id, role_id)
);

-- certifications (for lifeguards / maintenance)
CREATE TABLE certifications (
    cert_id NUMBER PRIMARY KEY,
    staff_id NUMBER NOT NULL,
    cert_name VARCHAR2(200) NOT NULL,
    issued_by VARCHAR2(200),
    issued_date DATE,
    expiry_date DATE,
    notes VARCHAR2(2000),
    CONSTRAINT fk_cert_staff FOREIGN KEY (staff_id) REFERENCES staff(staff_id) ON DELETE CASCADE
);

-- 4) POOLS, SLIDES, SLIDE_TYPES, ATTRACTIONS
CREATE TABLE pools (
    pool_id NUMBER PRIMARY KEY,
    name VARCHAR2(150) NOT NULL,
    depth_min NUMBER(5,2) NOT NULL,
    depth_max NUMBER(5,2) NOT NULL,
    capacity NUMBER NOT NULL CHECK (capacity >= 0),
    indoors CHAR(1) DEFAULT 'N' CHECK (indoors IN ('Y','N')),
    notes VARCHAR2(2000)
);

CREATE TABLE slide_types (
    slide_type_id NUMBER PRIMARY KEY,
    name VARCHAR2(100) NOT NULL,
    difficulty VARCHAR2(20) CHECK (difficulty IN ('Low','Medium','High','Extreme')),
    description VARCHAR2(2000)
);

CREATE TABLE slides (
    slide_id NUMBER PRIMARY KEY,
    name VARCHAR2(200) NOT NULL,
    slide_type_id NUMBER NOT NULL,
    length_m NUMBER(6,2) CHECK (length_m > 0),
    height_m NUMBER(6,2) CHECK (height_m >= 0),
    min_height_cm NUMBER(5) CHECK (min_height_cm >= 0),
    max_weight_kg NUMBER(5) CHECK (max_weight_kg >= 0),
    status VARCHAR2(20) DEFAULT 'OPEN' CHECK (status IN ('OPEN','CLOSED','MAINTENANCE','TEMP_CLOSED')),
    pool_id NUMBER,
    installation_date DATE,
    notes VARCHAR2(2000),
    CONSTRAINT fk_slide_type FOREIGN KEY (slide_type_id) REFERENCES slide_types(slide_type_id),
    CONSTRAINT fk_slide_pool FOREIGN KEY (pool_id) REFERENCES pools(pool_id)
);

-- Attractions generalize slides and other things (e.g., wave pool, lazy river)
CREATE TABLE attractions (
    attraction_id NUMBER PRIMARY KEY,
    name VARCHAR2(200) NOT NULL,
    attraction_type VARCHAR2(50) NOT NULL, -- e.g., 'SLIDE','POOL','WAVE_POOL','RIVER'
    object_id NUMBER, -- reference to slides.slide_id or pools.pool_id depending on type
    status VARCHAR2(20) DEFAULT 'OPEN' CHECK (status IN ('OPEN','CLOSED','MAINTENANCE','TEMP_CLOSED')),
    capacity NUMBER,
    notes VARCHAR2(2000)
);

-- 5) MAINTENANCE RECORDS
CREATE TABLE maintenance_records (
    maintenance_id NUMBER PRIMARY KEY,
    attraction_id NUMBER NOT NULL,
    reported_by NUMBER, -- staff
    report_date DATE DEFAULT SYSDATE NOT NULL,
    problem_description VARCHAR2(4000),
    action_taken VARCHAR2(4000),
    completed_date DATE,
    cost NUMBER(12,2) DEFAULT 0,
    CONSTRAINT fk_maint_attr FOREIGN KEY (attraction_id) REFERENCES attractions(attraction_id),
    CONSTRAINT fk_maint_staff FOREIGN KEY (reported_by) REFERENCES staff(staff_id)
);

-- 6) SCHEDULE (when attractions are open / closed / special events)
CREATE TABLE schedule (
    schedule_id NUMBER PRIMARY KEY,
    attraction_id NUMBER NOT NULL,
    start_datetime TIMESTAMP NOT NULL,
    end_datetime   TIMESTAMP NOT NULL,
    status VARCHAR2(20) DEFAULT 'OPEN' CHECK (status IN ('OPEN','CLOSED','PRIVATE','MAINTENANCE')),
    notes VARCHAR2(2000),
    CONSTRAINT fk_sched_attr FOREIGN KEY (attraction_id) REFERENCES attractions(attraction_id),
    CONSTRAINT chk_times CHECK (end_datetime > start_datetime)
);

-- 7) PRICING, TICKET TYPES, TICKETS, BOOKINGS
CREATE TABLE price_list (
    price_id NUMBER PRIMARY KEY,
    name VARCHAR2(200) NOT NULL,
    description VARCHAR2(2000),
    currency VARCHAR2(10) DEFAULT 'EUR',
    base_price NUMBER(12,2) NOT NULL CHECK (base_price >= 0),
    valid_from DATE DEFAULT SYSDATE,
    valid_to DATE,
    active CHAR(1) DEFAULT 'Y' CHECK (active IN ('Y','N'))
);

CREATE TABLE ticket_types (
    ticket_type_id NUMBER PRIMARY KEY,
    name VARCHAR2(200) NOT NULL,
    description VARCHAR2(2000),
    price_id NUMBER NOT NULL,
    duration_hours NUMBER(5,2), -- e.g., 2.5 hours or NULL for all-day
    age_min NUMBER,
    age_max NUMBER,
    CONSTRAINT fk_tt_price FOREIGN KEY (price_id) REFERENCES price_list(price_id)
);

CREATE TABLE visitors (
    visitor_id NUMBER PRIMARY KEY,
    first_name VARCHAR2(120),
    last_name VARCHAR2(120),
    date_of_birth DATE,
    email VARCHAR2(200),
    phone VARCHAR2(30),
    emergency_contact VARCHAR2(200),
    notes VARCHAR2(2000)
);

CREATE TABLE tickets (
    ticket_id NUMBER PRIMARY KEY,
    ticket_type_id NUMBER NOT NULL,
    visitor_id NUMBER,
    purchase_date DATE DEFAULT SYSDATE,
    valid_from TIMESTAMP,
    valid_to TIMESTAMP,
    price_paid NUMBER(12,2) NOT NULL CHECK (price_paid >= 0),
    status VARCHAR2(20) DEFAULT 'ACTIVE' CHECK (status IN ('ACTIVE','USED','REFUNDED','CANCELLED')),
    CONSTRAINT fk_ticket_type FOREIGN KEY (ticket_type_id) REFERENCES ticket_types(ticket_type_id),
    CONSTRAINT fk_ticket_visitor FOREIGN KEY (visitor_id) REFERENCES visitors(visitor_id)
);

CREATE TABLE bookings (
    booking_id NUMBER PRIMARY KEY,
    booking_ref VARCHAR2(30) UNIQUE,
    customer_name VARCHAR2(200),
    created_date DATE DEFAULT SYSDATE,
    total_amount NUMBER(12,2) DEFAULT 0,
    status VARCHAR2(20) DEFAULT 'CONFIRMED' CHECK (status IN ('CONFIRMED','CANCELLED','PENDING','COMPLETED')),
    notes VARCHAR2(2000)
);

-- booking_items allows reservations of attractions or tickets
CREATE TABLE booking_items (
    booking_item_id NUMBER PRIMARY KEY,
    booking_id NUMBER NOT NULL,
    attraction_id NUMBER, -- optional: reserved attraction (e.g., private cabana)
    ticket_id NUMBER,     -- optional: ticket assigned
    quantity NUMBER DEFAULT 1 CHECK (quantity > 0),
    unit_price NUMBER(12,2) DEFAULT 0,
    CONSTRAINT fk_bi_booking FOREIGN KEY (booking_id) REFERENCES bookings(booking_id) ON DELETE CASCADE,
    CONSTRAINT fk_bi_attraction FOREIGN KEY (attraction_id) REFERENCES attractions(attraction_id),
    CONSTRAINT fk_bi_ticket FOREIGN KEY (ticket_id) REFERENCES tickets(ticket_id)
);

-- memberships
CREATE TABLE memberships (
    membership_id NUMBER PRIMARY KEY,
    visitor_id NUMBER NOT NULL,
    membership_type VARCHAR2(100),
    start_date DATE DEFAULT SYSDATE,
    end_date DATE,
    recurring CHAR(1) DEFAULT 'N' CHECK (recurring IN ('Y','N')),
    status VARCHAR2(20) DEFAULT 'ACTIVE' CHECK (status IN ('ACTIVE','SUSPENDED','CANCELLED','EXPIRED')),
    CONSTRAINT fk_mem_visitor FOREIGN KEY (visitor_id) REFERENCES visitors(visitor_id)
);

-- 8) SHIFTS and staff_shifts (scheduling staff)
CREATE TABLE shifts (
    shift_id NUMBER PRIMARY KEY,
    shift_name VARCHAR2(100),
    start_time DATE, -- store date-only or use TIMESTAMP if spanning many dates
    end_time DATE,
    notes VARCHAR2(2000)
);

CREATE TABLE staff_shifts (
    staff_shift_id NUMBER PRIMARY KEY,
    staff_id NUMBER NOT NULL,
    shift_id NUMBER NOT NULL,
    shift_date DATE NOT NULL,
    assigned_at TIMESTAMP DEFAULT SYSTIMESTAMP,
    CONSTRAINT fk_ss_staff FOREIGN KEY (staff_id) REFERENCES staff(staff_id) ON DELETE CASCADE,
    CONSTRAINT fk_ss_shift FOREIGN KEY (shift_id) REFERENCES shifts(shift_id) ON DELETE CASCADE,
    CONSTRAINT u_staff_shift UNIQUE (staff_id, shift_id, shift_date)
);

-- 9) Payment records (simplified)
CREATE TABLE payments (
    payment_id NUMBER PRIMARY KEY,
    booking_id NUMBER,
    payment_date TIMESTAMP DEFAULT SYSTIMESTAMP,
    amount NUMBER(12,2) NOT NULL CHECK (amount >= 0),
    payment_method VARCHAR2(50), -- e.g., CASH, CARD, STRIPE
    reference VARCHAR2(200),
    CONSTRAINT fk_pay_booking FOREIGN KEY (booking_id) REFERENCES bookings(booking_id)
);

-- 10) Helpful indexes
CREATE INDEX idx_attractions_type ON attractions(attraction_type);
CREATE INDEX idx_schedule_attr_dt ON schedule(attraction_id, start_datetime, end_datetime);
CREATE INDEX idx_tickets_status ON tickets(status);