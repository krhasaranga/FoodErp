// ─── Auth ────────────────────────────────────────────────────────────────────
export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  tenantName: string;
  tenantSlug: string;
  firstName: string;
  lastName: string;
  email: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  email: string;
  firstName: string;
  lastName: string;
  tenantId: string;
  roles: string[];
}

// ─── Products ────────────────────────────────────────────────────────────────
export interface ProductCategory {
  id: string;
  name: string;
  description: string;
}

export interface Unit {
  id: string;
  name: string;
  symbol: string;
  isBaseUnit: boolean;
  isActive: boolean;
}

export interface Product {
  id: string;
  name: string;
  sku: string;
  categoryId: string;
  categoryName: string;
  unitId: string;
  unitName: string;
  costPrice: number;
  sellingPrice: number;
  reorderLevel: number;
  isActive: boolean;
}

// ─── Inventory ────────────────────────────────────────────────────────────────
export interface Warehouse {
  id: string;
  name: string;
  location: string;
  isActive: boolean;
}

export interface StockBalance {
  productId: string;
  productName: string;
  productCode: string;
  warehouseId: string;
  warehouseName: string;
  quantity: number;
  unitSymbol: string;
}

// ─── Production ───────────────────────────────────────────────────────────────
export interface Recipe {
  id: string;
  name: string;
  outputProductId: string;
  outputProductName: string;
  outputQuantity: number;
  lines: RecipeLine[];
}

export interface RecipeLine {
  id: string;
  inputProductId: string;
  inputProductName: string;
  quantity: number;
}

export interface ProductionOrder {
  id: string;
  recipeId: string;
  recipeName: string;
  plannedQuantity: number;
  status: string;
  scheduledDate: string;
  completedDate?: string;
}

// ─── Distribution ─────────────────────────────────────────────────────────────
export interface Customer {
  id: string;
  name: string;
  contactPerson: string;
  email: string;
  phone: string;
  address: string;
  customerType: string;
}

export interface SalesOrder {
  id: string;
  customerId: string;
  customerName: string;
  orderDate: string;
  status: string;
  totalAmount: number;
  lines: SalesOrderLine[];
}

export interface SalesOrderLine {
  id: string;
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

// ─── Invoicing ────────────────────────────────────────────────────────────────
export interface Invoice {
  id: string;
  invoiceNumber: string;
  customerId: string;
  customerName: string;
  invoiceDate: string;
  dueDate: string;
  status: string;
  totalAmount: number;
  paidAmount: number;
  balanceDue: number;
  isOverdue: boolean;
}

export interface Payment {
  id: string;
  invoiceId: string;
  invoiceNumber: string;
  amount: number;
  paymentDate: string;
  paymentMethod: string;
  reference: string;
}

// ─── Employees ────────────────────────────────────────────────────────────────
export interface Employee {
  id: string;
  employeeCode: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  department: string;
  position: string;
  employmentType: string;
  hireDate: string;
  salary: number;
  isActive: boolean;
}

// ─── Reports ──────────────────────────────────────────────────────────────────
export interface DashboardReport {
  totalProducts: number;
  totalCustomers: number;
  totalEmployees: number;
  totalWarehouses: number;
  monthSalesAmount: number;
  monthInvoicesCount: number;
  overdueInvoicesCount: number;
  overdueInvoicesAmount: number;
  pendingProductionOrders: number;
  lowStockProducts: number;
}
