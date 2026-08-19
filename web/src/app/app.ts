import { afterNextRender, ChangeDetectorRef, Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { finalize, forkJoin, retry, timer } from 'rxjs';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';
import { ButtonModule } from 'primeng/button';
import { ChartModule } from 'primeng/chart';
import { DialogModule } from 'primeng/dialog';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ToggleSwitchModule } from 'primeng/toggleswitch';
import { TooltipModule } from 'primeng/tooltip';

type View =
  'dashboard' | 'products' | 'product-form' | 'product-edit' | 'invoices' | 'invoice-form';

interface Product {
  id: string;
  code: string;
  description: string;
  availableQuantity: number;
  unitPrice: number;
  totalAvailableValue: number;
  totalConsumedValue: number;
}

interface InvoiceItem {
  productId: string;
  productCode: string;
  productDescription: string;
  quantity: number;
}

interface Invoice {
  id: string;
  number: number;
  status: number;
  items: InvoiceItem[];
  createdAt: string;
}

@Component({
  selector: 'app-root',
  imports: [
    CommonModule,
    FormsModule,
    ButtonModule,
    ChartModule,
    DialogModule,
    InputNumberModule,
    InputTextModule,
    SelectModule,
    TableModule,
    TagModule,
    ToggleSwitchModule,
    TooltipModule,
  ],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  private static readonly themeStorageKey = 'korp-theme-v2';
  view: View = 'dashboard';
  products: Product[] = [];
  invoices: Invoice[] = [];
  loading = false;
  message = '';
  error = '';
  private successTimeout?: ReturnType<typeof setTimeout>;
  private errorTimeout?: ReturnType<typeof setTimeout>;
  darkMode = false;

  product = { code: '', description: '', availableQuantity: 0, unitPrice: 0 };
  editingProductId: string | null = null;
  showDeleteConfirmation = false;
  showCancelInvoiceConfirmation = false;
  invoiceToCancel: Invoice | null = null;
  invoiceItem = { productId: '', quantity: 1 };
  invoiceItems: InvoiceItem[] = [];

  constructor(
    private readonly http: HttpClient,
    private readonly changeDetector: ChangeDetectorRef,
  ) {
    afterNextRender(() => {
      this.darkMode = localStorage.getItem(App.themeStorageKey) === 'dark';
      this.applyTheme();
      this.refresh();
    });
  }

  get productsWithLowStock(): number {
    return this.products.filter((product) => product.availableQuantity <= 2).length;
  }

  get openInvoices(): number {
    return this.invoices.filter((invoice) => invoice.status === 1).length;
  }

  invoiceStatusLabel(status: number): string {
    return status === 1 ? 'Aberta' : status === 2 ? 'Fechada' : 'Cancelada';
  }

  invoiceStatusSeverity(status: number): 'success' | 'secondary' | 'danger' {
    return status === 1 ? 'success' : status === 2 ? 'secondary' : 'danger';
  }

  get stockChart(): Product[] {
    return [...this.products]
      .sort((left, right) => right.availableQuantity - left.availableQuantity)
      .slice(0, 6);
  }

  get maxStock(): number {
    return Math.max(...this.stockChart.map((product) => product.availableQuantity), 1);
  }

  get productOptions(): { label: string; value: string }[] {
    return this.products.map((product) => ({
      label: `${product.code} - ${product.description} (${product.availableQuantity})`,
      value: product.id,
    }));
  }

  get stockChartData() {
    return {
      labels: this.stockChart.map((product) => product.code),
      datasets: [
        {
          label: 'Saldo disponível',
          data: this.stockChart.map((product) => product.availableQuantity),
        backgroundColor: this.darkMode ? '#a78bfa' : '#7c3aed',
        hoverBackgroundColor: this.darkMode ? '#c4b5fd' : '#8b5cf6',
          borderRadius: 8,
          borderSkipped: false,
          maxBarThickness: 46,
        },
      ],
    };
  }

  get stockChartOptions() {
    const textColor = this.darkMode ? '#ddd6fe' : '#475569';
    const gridColor = this.darkMode ? 'rgba(221, 214, 254, .12)' : 'rgba(71, 85, 105, .12)';
    return {
      maintainAspectRatio: false,
      plugins: { legend: { display: false } },
      scales: {
        x: { grid: { display: false }, ticks: { color: textColor } },
        y: {
          beginAtZero: true,
          grid: { color: gridColor },
          ticks: { color: textColor, precision: 0 },
        },
      },
    };
  }

  applyTheme(): void {
    document.documentElement.classList.toggle('app-dark', this.darkMode);
    localStorage.setItem(App.themeStorageKey, this.darkMode ? 'dark' : 'light');
  }

  get isProductFormValid(): boolean {
    return (
      this.product.code.trim().length > 0 &&
      this.product.code.trim().length <= 20 &&
      this.product.description.trim().length > 0 &&
      this.product.description.trim().length <= 100 &&
      Number.isFinite(this.product.availableQuantity) &&
      this.product.availableQuantity >= 0 &&
      Number.isFinite(this.product.unitPrice) &&
      this.product.unitPrice >= 0
    );
  }

  navigate(view: View): void {
    this.view = view;
    this.message = '';
    this.error = '';

    if (view === 'dashboard' || view === 'products' || view === 'invoices') {
      this.refresh();
    }
  }

  refresh(): void {
    this.loading = true;
    this.error = '';
    const cacheBust = `?_=${Date.now()}`;

    forkJoin({
      products: this.http.get<Product[]>(`http://localhost:5219/api/products${cacheBust}`),
      invoices: this.http.get<Invoice[]>(`http://localhost:5290/api/invoices${cacheBust}`),
    })
      .pipe(
        retry({ count: 3, delay: () => timer(1000) }),
        finalize(() => {
          this.loading = false;
          this.changeDetector.detectChanges();
        }),
      )
      .subscribe({
        next: ({ products, invoices }) => {
          this.products = products;
          this.invoices = invoices;
          this.changeDetector.detectChanges();
        },
        error: (error) => this.fail(error),
      });
  }

  createProduct(): void {
    if (!this.isProductFormValid) {
      this.showError('Preencha os campos obrigatórios com valores válidos.');
      return;
    }

    this.http.post<Product>('http://localhost:5219/api/products', this.product).subscribe({
      next: (product) => {
        this.products = [...this.products, product].sort((left, right) =>
          left.code.localeCompare(right.code),
        );
        this.product = { code: '', description: '', availableQuantity: 0, unitPrice: 0 };
        this.navigate('products');
        this.showSuccess('Produto cadastrado com sucesso.');
        this.changeDetector.detectChanges();
      },
      error: (error) => this.fail(error),
    });
  }

  openNewProduct(): void {
    this.product = { code: '', description: '', availableQuantity: 0, unitPrice: 0 };
    this.editingProductId = null;
    this.navigate('product-form');
  }

  editProduct(product: Product): void {
    this.editingProductId = product.id;
    this.product = {
      code: product.code,
      description: product.description,
      availableQuantity: product.availableQuantity,
      unitPrice: product.unitPrice,
    };
    this.navigate('product-edit');
  }

  updateProduct(): void {
    if (!this.editingProductId || !this.isProductFormValid) {
      this.showError('Preencha os campos obrigatórios com valores válidos.');
      return;
    }

    this.http
      .put<Product>(`http://localhost:5219/api/products/${this.editingProductId}`, this.product)
      .subscribe({
        next: (product) => {
          this.products = this.products
            .map((item) => (item.id === product.id ? product : item))
            .sort((left, right) => left.code.localeCompare(right.code));
          this.navigate('products');
          this.showSuccess('Produto atualizado com sucesso.');
          this.changeDetector.detectChanges();
        },
        error: (error) => this.fail(error),
      });
  }

  deleteProduct(): void {
    if (!this.editingProductId) {
      return;
    }

    this.showDeleteConfirmation = true;
  }

  cancelDeleteProduct(): void {
    this.showDeleteConfirmation = false;
  }

  confirmDeleteProduct(): void {
    if (!this.editingProductId) {
      return;
    }

    this.http.delete(`http://localhost:5219/api/products/${this.editingProductId}`).subscribe({
      next: () => {
        this.showDeleteConfirmation = false;
        this.products = this.products.filter((product) => product.id !== this.editingProductId);
        this.navigate('products');
        this.showSuccess('Produto excluído com sucesso.');
        this.changeDetector.detectChanges();
      },
      error: (error) => this.fail(error),
    });
  }

  addInvoiceItem(): void {
    const product = this.products.find((item) => item.id === this.invoiceItem.productId);
    if (!product) {
      this.showError('Selecione um produto para adicionar à nota.');
      return;
    }

    if (this.invoiceItem.quantity <= 0) {
      this.showError('A quantidade deve ser maior que zero.');
      return;
    }

    const existingItem = this.invoiceItems.find((item) => item.productId === product.id);
    const requestedQuantity = (existingItem?.quantity ?? 0) + this.invoiceItem.quantity;
    if (requestedQuantity > product.availableQuantity) {
      this.showError(
        `Quantidade insuficiente para ${product.code}. Disponível: ${product.availableQuantity}; solicitada: ${requestedQuantity}.`,
      );
      return;
    }

    if (existingItem) {
      existingItem.quantity = requestedQuantity;
    } else {
      this.invoiceItems = [
        ...this.invoiceItems,
        {
          productId: product.id,
          productCode: product.code,
          productDescription: product.description,
          quantity: this.invoiceItem.quantity,
        },
      ];
    }

    this.invoiceItem = { productId: '', quantity: 1 };
    this.error = '';
  }

  removeInvoiceItem(productId: string): void {
    this.invoiceItems = this.invoiceItems.filter((item) => item.productId !== productId);
  }

  createInvoice(): void {
    if (this.invoiceItems.length === 0) {
      this.showError('Adicione pelo menos um produto à nota.');
      return;
    }

    const itemWithoutStock = this.invoiceItems.find((item) => {
      const product = this.products.find((product) => product.id === item.productId);
      return !product || item.quantity > product.availableQuantity;
    });

    if (itemWithoutStock) {
      const product = this.products.find((product) => product.id === itemWithoutStock.productId);
      this.showError(
        `Quantidade insuficiente para ${itemWithoutStock.productCode}. Disponível: ${product?.availableQuantity ?? 0}; solicitada: ${itemWithoutStock.quantity}.`,
      );
      return;
    }

    this.http
      .post<Invoice>('http://localhost:5290/api/invoices', { items: this.invoiceItems })
      .subscribe({
        next: (invoice) => {
          this.invoices = [invoice, ...this.invoices];
          this.invoiceItems = [];
          this.navigate('invoices');
          this.showSuccess(`Nota #${invoice.number} criada com sucesso.`);
          this.changeDetector.detectChanges();
        },
        error: (error) => this.fail(error),
      });
  }

  print(invoice: Invoice): void {
    this.http
      .post(`http://localhost:5290/api/invoices/${invoice.id}/print`, {
        idempotencyKey: `ui-${invoice.id}`,
      })
      .subscribe({
        next: () => {
          this.showSuccess(`Nota #${invoice.number} impressa e fechada.`);
          const updatedInvoice: Invoice = { ...invoice, status: 2 };
          this.exportInvoicePdf(updatedInvoice);
          this.refresh();
        },
        error: (error) => this.fail(error),
      });
  }

  exportInvoicePdf(invoice: Invoice): void {
    const doc = new jsPDF();
    const pageWidth = doc.internal.pageSize.getWidth();

    // Primary Brand Header Banner
    doc.setFillColor(124, 58, 237);
    doc.rect(0, 0, pageWidth, 24, 'F');

    doc.setFont('helvetica', 'bold');
    doc.setFontSize(15);
    doc.setTextColor(255, 255, 255);
    doc.text('INVOICE - GESTÃO OPERACIONAL', 14, 16);

    doc.setFont('helvetica', 'normal');
    doc.setFontSize(9);
    doc.text('Documento Auxiliar de Venda / Nota', pageWidth - 14, 16, { align: 'right' });

    // Invoice Header Details Box
    doc.setFillColor(248, 250, 252);
    doc.setDrawColor(226, 232, 240);
    doc.roundedRect(14, 30, pageWidth - 28, 30, 2, 2, 'FD');

    doc.setTextColor(100, 116, 139);
    doc.setFontSize(8.5);
    doc.text('NÚMERO DA NOTA', 20, 38);
    doc.text('DATA DE EMISSÃO', 80, 38);
    doc.text('STATUS', 145, 38);

    doc.setFont('helvetica', 'bold');
    doc.setFontSize(13);
    doc.setTextColor(15, 23, 42);
    doc.text(`#${invoice.number}`, 20, 46);

    const formattedDate = invoice.createdAt
      ? new Date(invoice.createdAt).toLocaleString('pt-BR')
      : new Date().toLocaleString('pt-BR');
    doc.setFontSize(10.5);
    doc.text(formattedDate, 80, 46);

    const statusText =
      invoice.status === 1 ? 'Aberta' : invoice.status === 2 ? 'Fechada / Emitida' : 'Cancelada';
    doc.setTextColor(
      invoice.status === 2 ? 22 : 124,
      invoice.status === 2 ? 101 : 58,
      invoice.status === 2 ? 52 : 237,
    );
    doc.text(statusText, 145, 46);

    doc.setFont('helvetica', 'normal');
    doc.setFontSize(8);
    doc.setTextColor(148, 163, 184);
    doc.text(`Identificador: ${invoice.id}`, 20, 55);

    // Prepare table rows
    let totalQuantity = 0;
    let totalValue = 0;

    const tableBody = invoice.items.map((item, index) => {
      totalQuantity += item.quantity;
      const product = this.products.find(
        (p) => p.id === item.productId || p.code === item.productCode,
      );
      const unitPrice = product?.unitPrice ?? 0;
      const itemTotal = unitPrice * item.quantity;
      totalValue += itemTotal;

      return [
        (index + 1).toString(),
        item.productCode,
        item.productDescription,
        item.quantity.toLocaleString('pt-BR'),
        unitPrice > 0 ? this.formatCurrency(unitPrice) : '-',
        itemTotal > 0 ? this.formatCurrency(itemTotal) : '-',
      ];
    });

    // Items Table using autoTable
    autoTable(doc, {
      startY: 66,
      head: [['#', 'Código', 'Descrição do Produto', 'Qtd', 'Vl. Unitário', 'Vl. Total']],
      body: tableBody,
      theme: 'grid',
      headStyles: {
        fillColor: [124, 58, 237],
        textColor: [255, 255, 255],
        fontStyle: 'bold',
        fontSize: 9,
      },
      styles: {
        fontSize: 9,
        cellPadding: 4,
        textColor: [51, 65, 85],
      },
      columnStyles: {
        0: { cellWidth: 12, halign: 'center' },
        1: { cellWidth: 32, fontStyle: 'bold' },
        2: { cellWidth: 'auto' },
        3: { cellWidth: 20, halign: 'right' },
        4: { cellWidth: 30, halign: 'right' },
        5: { cellWidth: 32, halign: 'right' },
      },
    });

    // Summary Box at Bottom
    const finalY = (doc as any).lastAutoTable?.finalY ? (doc as any).lastAutoTable.finalY + 8 : 120;
    const summaryWidth = 85;
    const summaryX = pageWidth - 14 - summaryWidth;

    doc.setFillColor(248, 250, 252);
    doc.setDrawColor(226, 232, 240);
    doc.roundedRect(summaryX, finalY, summaryWidth, 28, 2, 2, 'FD');

    doc.setFontSize(8.5);
    doc.setFont('helvetica', 'normal');
    doc.setTextColor(100, 116, 139);
    doc.text('Total de Itens:', summaryX + 6, finalY + 8);
    doc.text(invoice.items.length.toString(), summaryX + summaryWidth - 6, finalY + 8, {
      align: 'right',
    });

    doc.text('Qtd. Total de Peças:', summaryX + 6, finalY + 16);
    doc.text(totalQuantity.toLocaleString('pt-BR'), summaryX + summaryWidth - 6, finalY + 16, {
      align: 'right',
    });

    if (totalValue > 0) {
      doc.setFont('helvetica', 'bold');
      doc.setTextColor(15, 23, 42);
      doc.text('Valor Total:', summaryX + 6, finalY + 24);
      doc.text(this.formatCurrency(totalValue), summaryX + summaryWidth - 6, finalY + 24, {
        align: 'right',
      });
    }

    // Document Footer
    const pageHeight = doc.internal.pageSize.getHeight();
    doc.setFont('helvetica', 'italic');
    doc.setFontSize(8);
    doc.setTextColor(148, 163, 184);
    doc.text(
      `Documento gerado eletronicamente em ${new Date().toLocaleString('pt-BR')}`,
      14,
      pageHeight - 10,
    );
    doc.text('Invoice System', pageWidth - 14, pageHeight - 10, { align: 'right' });

    // Download PDF
    doc.save(`nota_fiscal_${invoice.number}.pdf`);
  }

  requestCancelInvoice(invoice: Invoice): void {
    this.invoiceToCancel = invoice;
    this.showCancelInvoiceConfirmation = true;
  }

  cancelInvoice(): void {
    if (!this.invoiceToCancel) {
      return;
    }

    const invoice = this.invoiceToCancel;
    this.http.delete<Invoice>(`http://localhost:5290/api/invoices/${invoice.id}`).subscribe({
      next: (cancelledInvoice) => {
        this.invoices = this.invoices.map((item) =>
          item.id === cancelledInvoice.id ? cancelledInvoice : item,
        );
        this.showCancelInvoiceConfirmation = false;
        this.invoiceToCancel = null;
        this.showSuccess(`Nota #${invoice.number} cancelada com sucesso.`);
        this.changeDetector.detectChanges();
      },
      error: (error) => this.fail(error),
    });
  }

  cancelInvoiceCancellation(): void {
    this.showCancelInvoiceConfirmation = false;
    this.invoiceToCancel = null;
  }

  private fail(error: any): void {
    this.loading = false;
    const validationErrors = Object.values(error?.error?.errors ?? {})
      .flat()
      .join(' ');
    const message = error?.error?.detail || error?.error?.title || validationErrors;
    this.showError(this.friendlyErrorMessage(message));
  }

  private showSuccess(message: string): void {
    if (this.successTimeout) {
      clearTimeout(this.successTimeout);
    }

    this.message = message;
    this.error = '';
    this.successTimeout = setTimeout(() => {
      this.message = '';
      this.changeDetector.detectChanges();
    }, 5000);
  }

  private showError(message: string): void {
    if (this.errorTimeout) {
      clearTimeout(this.errorTimeout);
    }

    this.error = message;
    this.message = '';
    this.changeDetector.detectChanges();
    this.errorTimeout = setTimeout(() => {
      this.error = '';
      this.changeDetector.detectChanges();
    }, 7000);
  }

  private friendlyErrorMessage(message?: string): string {
    const messages: Record<string, string> = {
      'inventory-stock-rejected': 'O estoque não pôde ser baixado. Verifique a quantidade disponível.',
      'inventory-service-unavailable': 'O serviço de estoque está indisponível. Tente novamente em instantes.',
    };

    return messages[message ?? ''] || message || 'Não foi possível concluir a operação. Confirme que as APIs estão em execução.';
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value);
  }
}
