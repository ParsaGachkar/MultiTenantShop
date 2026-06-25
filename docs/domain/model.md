# Domain Model

> Document DB model (LiteDB / MongoDB). PK/FK markers in diagrams represent logical document references, not database constraints.

## Entity Relationship Diagram

```mermaid
erDiagram
    Tenant ||--o{ Product : "owns"
    Tenant ||--o{ Order : "owns"
    Tenant ||--o{ Customer : "owns"
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
    Product ||--o{ ProductPrice : "priced in"
    Product {
        ulid ProductId PK
        ulid TenantId
        ulid CategoryId
        string Name
        string Description
        string ImageUrl
        string SKU
        int StockQuantity
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

    ProductPrice {
        ulid ProductId
        decimal Amount
        string Currency
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

    Order ||--o{ OrderItem : "contains"
    Order ||--|| Payment : "has"
    Order {
        ulid OrderId PK
        ulid TenantId
        ulid CustomerId
        string OrderNumber
        string Status
        decimal SubTotal
        decimal TaxTotal
        decimal ShippingTotal
        decimal GrandTotal
        string Currency
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

    Customer ||--o{ Order : "places"
    Customer ||--o{ Cart : "has"
    Customer {
        ulid CustomerId PK
        ulid TenantId
        string Email
        string FirstName
        string LastName
        string Phone
        datetime CreatedAt
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
    + AdjustStock(int quantity)
    + GetPrice(string currency): Money
    + ApplyDiscount(decimal percent)
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

package "Order Context" {
  class Order {
    - OrderId: ulid
    - TenantId: ulid
    - OrderNumber: string
    - Status: OrderStatus
    - Items: List~OrderItem~
    + AddItem(Product, int)
    + CalculateTotal(): Money
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
