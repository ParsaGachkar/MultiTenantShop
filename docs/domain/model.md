# Domain Model

> Document DB model (LiteDB / MongoDB). PK/FK markers in diagrams represent logical document references, not database constraints.

## Entity Relationship Diagram

```mermaid
erDiagram
    Tenant ||--o{ Product : "owns"
    Tenant ||--o{ Order : "owns"
    Tenant ||--o{ Customer : "owns"
    Tenant ||--o{ Coupon : "owns"
    Tenant {
        ulid TenantId PK
        string Name
        string Slug
        string Plan
        string DefaultCurrency
        string DefaultLanguage
        string[] SupportedCurrencies
        string CustomDomain
        string ThemeSettings
        string ConnectionString
        datetime CreatedAt
    }

    Category ||--o{ Product : "contains"
    Category {
        ulid CategoryId PK
        ulid TenantId
        string Name
        string Slug
        string Description
    }

    Product ||--o{ OrderItem : "contains"
    Product ||--o{ CartItem : "contains"
    Product ||--o{ ProductVariant : "has"
    Product ||--o{ InventoryMovement : "tracks"
    Product {
        ulid ProductId PK
        ulid TenantId
        ulid CategoryId
        string Name
        string Description
        string ImageUrl
        string SKU
        int StockQuantity
        int LowStockThreshold
        bool IsActive
        datetime CreatedAt
    }

    ProductVariant {
        ulid VariantId PK
        ulid ProductId
        string Name
        string Value
        decimal PriceAdjustment
        int StockQuantity
    }

    InventoryMovement {
        ulid MovementId PK
        ulid ProductId
        ulid VariantId
        int Quantity
        string Reason
        string ReferenceId
        string Note
        datetime CreatedAt
    }

    Cart ||--o{ CartItem : "contains"
    Cart {
        ulid CartId PK
        ulid TenantId
        ulid CustomerId
        datetime CreatedAt
        datetime UpdatedAt
    }

    CartItem {
        ulid CartItemId PK
        ulid CartId
        ulid ProductId
        ulid VariantId
        int Quantity
        decimal UnitPrice
    }

    Coupon ||--o{ Order : "applied to"
    Coupon {
        ulid CouponId PK
        ulid TenantId
        string Code
        string DiscountType
        decimal DiscountValue
        decimal MinOrderAmount
        int MaxUses
        int CurrentUses
        datetime ExpiresAt
        bool IsActive
        datetime CreatedAt
    }

    Customer ||--o{ Order : "places"
    Customer ||--o{ Cart : "has"
    Customer ||--o{ CustomerAddress : "has"
    Customer {
        ulid CustomerId PK
        ulid TenantId
        string Email
        string FirstName
        string LastName
        string Phone
        datetime CreatedAt
    }

    CustomerAddress {
        ulid AddressId PK
        ulid CustomerId
        string Label
        string Province
        string City
        string PostalCode
        string Street
        string FullText
        decimal Latitude
        decimal Longitude
        bool IsDefault
    }

    Order ||--o{ OrderItem : "contains"
    Order ||--|| Payment : "has"
    Order ||--o{ Shipment : "has"
    Order ||--o{ Refund : "has"
    Order {
        ulid OrderId PK
        ulid TenantId
        ulid CustomerId
        string OrderNumber
        string Status
        decimal SubTotal
        decimal TaxTotal
        decimal ShippingTotal
        decimal DiscountAmount
        decimal GrandTotal
        string Currency
        ulid CouponId
        string ShippingAddress
        string BillingAddress
        string CultureName
        datetime CreatedAt
        datetime UpdatedAt
    }

    OrderItem {
        ulid OrderItemId PK
        ulid OrderId
        ulid ProductId
        ulid VariantId
        string ProductName
        string SKU
        int Quantity
        decimal UnitPrice
        decimal TotalPrice
    }

    Payment {
        ulid PaymentId PK
        ulid OrderId
        string PaymentMethod
        string TransactionId
        decimal Amount
        string Currency
        string Status
        datetime PaidAt
    }

    Shipment {
        ulid ShipmentId PK
        ulid OrderId
        string Carrier
        string TrackingCode
        string Status
        string ShippingAddress
        datetime ShippedAt
        datetime DeliveredAt
    }

    Refund {
        ulid RefundId PK
        ulid OrderId
        ulid PaymentId
        string Reason
        string Status
        decimal Amount
        bool Restock
        datetime CreatedAt
    }
```

## Tenant Document (stored in shared DB)

```json
{
  "_id": "a1b2c3d4",
  "slug": "acme-corp",
  "name": "Acme Corporation",
  "plan": "Professional",
  "defaultCurrency": "USD",
  "defaultLanguage": "en",
  "supportedCurrencies": ["USD", "EUR", "GBP"],
  "customDomain": "acme.myshop.com",
  "themeSettings": {
    "primaryColor": "#ff6600",
    "logoUrl": "https://cdn.myshop.com/acme/logo.png",
    "fontFamily": "Inter"
  },
  "connectionString": null,
  "createdAt": "2026-06-25T00:00:00Z"
}
```

## Domain Class Diagram (PlantUML)

```plantuml
@startuml
package "Tenant Boundary" {
  class Tenant {
    - TenantId: ulid
    - Name: string
    - Slug: string
    - Plan: string
    - DefaultCurrency: string
    - DefaultLanguage: string
    - SupportedCurrencies: List~string~
    - CustomDomain: string
    - ThemeSettings: string
    - ConnectionString: string
    - CreatedAt: DateTime
  }

  interface ITenantScoped {
    + TenantId: ulid
  }
}

package "Catalog Context" {
  class Product {
    - ProductId: ulid
    - TenantId: ulid
    - Name: string
    - Prices: List~Money~
    - Sku: string
    - StockQuantity: int
    - LowStockThreshold: int
    + AdjustStock(int quantity, string reason)
    + GetPrice(string currency): Money
    + IsLowStock(): bool
  }

  class Money {
    - Amount: decimal
    - Currency: string
    + Add(Money other): Money
    + Multiply(decimal factor): Money
    + ConvertTo(targetCurrency, rate): Money
  }

  class Category {
    - CategoryId: ulid
    - Name: string
    - Slug: string
  }

  class ProductVariant {
    - VariantId: ulid
    - Name: string
    - Value: string
    - PriceAdjustment: Money
  }

  Product "1" --> "*" ProductVariant
  Product "*" --> "1" Category
  Product ..|> ITenantScoped
  Product *--> "*" Money : prices
}

package "Inventory Context" {
  class InventoryMovement {
    - MovementId: ulid
    - ProductId: ulid
    - VariantId: ulid
    - Quantity: int
    - Reason: MovementReason
    - ReferenceId: string
    - Note: string
    - CreatedAt: DateTime
  }

  enum MovementReason {
    Purchase
    Sale
    Adjustment
    Return
    Transfer
  }

  Product "1" --> "*" InventoryMovement
}

package "Order Context" {
  class Order {
    - OrderId: ulid
    - TenantId: ulid
    - OrderNumber: string
    - Status: OrderStatus
    - Items: List~OrderItem~
    - DiscountAmount: Money
    - CouponId: ulid
    + AddItem(Product, int)
    + CalculateTotal(): Money
    + ApplyCoupon(Coupon): DiscountResult
    + Submit()
    + Cancel()
  }

  enum OrderStatus {
    Pending
    Confirmed
    Processing
    Shipped
    Delivered
    Cancelled
    Refunded
  }

  class OrderItem {
    - ProductId: ulid
    - ProductName: string
    - Quantity: int
    - UnitPrice: Money
  }

  Order "1" --> "*" OrderItem
  Order ..|> ITenantScoped
  Order *--> Money
}

package "Payment Context" {
  class Payment {
    - PaymentId: ulid
    - OrderId: ulid
    - Method: PaymentMethod
    - TransactionId: string
    - Amount: Money
    - Status: PaymentStatus
  }

  enum PaymentMethod {
    ZarinPal
  }

  enum PaymentStatus {
    Pending
    Completed
    Failed
    Refunded
    PartiallyRefunded
  }

  Payment *--> Money
}

package "Promotion Context" {
  class Coupon {
    - CouponId: ulid
    - TenantId: ulid
    - Code: string
    - DiscountType: DiscountType
    - DiscountValue: decimal
    - MinOrderAmount: Money
    - MaxUses: int
    - CurrentUses: int
    - ExpiresAt: DateTime
    - IsActive: bool
    + CanApply(Order): bool
    + CalculateDiscount(Order): Money
  }

  enum DiscountType {
    Percentage
    Fixed
  }

  Coupon ..|> ITenantScoped
}

package "Cart Context" {
  class Cart {
    - CartId: ulid
    - Items: List~CartItem~
    + AddProduct(Product, int)
    + RemoveProduct(ProductId)
    + Clear()
    + CalculateTotal(): Money
  }

  class CartItem {
    - ProductId: ulid
    - ProductName: string
    - Quantity: int
    - UnitPrice: Money
  }

  Cart "1" --> "*" CartItem
  Cart *--> Money
}

package "Customer Context" {
  class Customer {
    - CustomerId: ulid
    - TenantId: ulid
    - Email: string
    - FirstName: string
    - LastName: string
    - Phone: string
    - Addresses: List~Address~
    + GetDefaultAddress(): Address
  }

  class Address {
    - Label: string
    - Province: string
    - City: string
    - PostalCode: string
    - Street: string
    - FullText: string
    - Latitude: decimal
    - Longitude: decimal
    + Format(bool persian): string
  }

  Customer *--> "*" Address
  Customer ..|> ITenantScoped
}

package "Fulfillment Context" {
  class Shipment {
    - ShipmentId: ulid
    - OrderId: ulid
    - Carrier: string
    - TrackingCode: string
    - Status: ShipmentStatus
    - ShippingAddress: Address
    - ShippedAt: DateTime
    - DeliveredAt: DateTime
  }

  enum ShipmentStatus {
    Pending
    PickedUp
    InTransit
    Delivered
    Failed
  }

  class Refund {
    - RefundId: ulid
    - OrderId: ulid
    - PaymentId: ulid
    - Reason: string
    - Status: RefundStatus
    - Amount: Money
    - Restock: bool
    - CreatedAt: DateTime
  }

  enum RefundStatus {
    Pending
    Approved
    Rejected
    Completed
  }

  Order "1" --> "*" Shipment
  Order "1" --> "*" Refund
  Shipment *--> Address
  Refund *--> Money
}

hide empty members
@enduml
```

## Key Workflows

### Tenant Onboarding

```mermaid
stateDiagram-v2
    [*] --> Signup: Tenant registers
    Signup --> PlanSelection: Email verified
    PlanSelection --> Payment: Plan chosen
    Payment --> DNS: Payment successful
    DNS --> Ready: DNS configured
    Ready --> [*]
```

### Order Placement Flow

```mermaid
stateDiagram-v2
    [*] --> Browsing: Customer browses catalog
    Browsing --> Cart: Adds item to cart
    Cart --> LoginRequired: Proceeds to checkout
    LoginRequired --> Authenticated: Logs in / Registers
    LoginRequired --> Authenticated: Continues as guest
    Authenticated --> EnterAddress: Shipping address form
    EnterAddress --> ReviewOrder: Address confirmed
    ReviewOrder --> PaymentPending: Submits order
    PaymentPending --> PaymentFailed: Payment declined
    PaymentPending --> Confirmed: Payment successful
    PaymentFailed --> ReviewOrder: Retry payment
    Confirmed --> Processing: Fulfillment starts
    Processing --> Shipped: Shipped
    Shipped --> Delivered: Customer received
    Delivered --> [*]

    Confirmed --> Cancelled: Customer cancels
    Processing --> Cancelled: Admin cancels
    Delivered --> Refunded: Refund issued
```

### Payment Processing

```mermaid
sequenceDiagram
    participant Client
    participant API as Orders API
    participant App as Application
    participant Domain
    participant Payment as Payment Gateway
    participant Bus as Message Bus
    participant Email
    participant SMS

    Client->>API: POST /orders
    API->>App: PlaceOrderCommand
    App->>Domain: Create Order (Pending)
    Domain-->>App: OrderCreatedEvent
    App->>Payment: ProcessPayment(amount, callbackUrl)
    Payment-->>App: PaymentSucceeded(txId)
    App->>Domain: Mark Paid
    App->>Bus: Publish OrderConfirmedEvent
    Bus->>Email: Send email confirmation
    Bus->>SMS: Send SMS notification
    App-->>API: OrderId + status
    API-->>Client: 201 Created
```

### Refund Flow

```mermaid
sequenceDiagram
    participant Admin as Admin Panel
    participant API as Orders API
    participant App as Application
    participant Domain
    participant Payment as Payment Gateway
    participant Inventory

    Admin->>API: POST /orders/{id}/refund
    API->>App: RefundOrderCommand
    App->>Domain: Create Refund (Pending)
    App->>Payment: ProcessRefund(paymentId, amount)
    alt Refund Approved
        Payment-->>App: RefundSucceeded(txId)
        App->>Domain: Approve Refund
        alt Restock Enabled
            App->>Inventory: AdjustStock(productId, +quantity)
            App->>Inventory: Record Movement (Return)
        end
        App->>Domain: Mark Refund Completed
        Domain-->>App: OrderStatusChanged(Refunded)
    else Refund Declined
        Payment-->>App: RefundFailed(reason)
        App->>Domain: Reject Refund
    end
    App-->>API: RefundResult
    API-->>Admin: Refund status
```
