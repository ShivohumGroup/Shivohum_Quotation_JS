const PAGE_WIDTH_PX = 794;
const PAGE_HEIGHT_PX = 1123;
const PAGE_PADDING_PX = 40;
const MAX_CONTENT_HEIGHT = PAGE_HEIGHT_PX - PAGE_PADDING_PX * 2;
const QUOTE_WATERMARK = '/logo-dark.png';
export const INVOICE_WATERMARK = '/invoice-logo.png';

function createPageShell(watermarkSrc: string): HTMLDivElement {
  const page = document.createElement('div');
  page.className = 'proposal-pdf-page';
  page.style.cssText = [
    `width:${PAGE_WIDTH_PX}px`,
    `min-height:${PAGE_HEIGHT_PX}px`,
    `height:${PAGE_HEIGHT_PX}px`,
    'box-sizing:border-box',
    `padding:${PAGE_PADDING_PX}px`,
    'background:#ffffff',
    'color:#1a1f24',
    'position:relative',
    'overflow:hidden',
    "font-family: 'Source Sans 3', 'Segoe UI', sans-serif"
  ].join(';');

  const watermark = document.createElement('div');
  watermark.setAttribute('data-pdf-page-watermark', 'true');
  watermark.style.cssText =
    'position:absolute;inset:0;display:flex;align-items:center;justify-content:center;pointer-events:none;z-index:0;opacity:0.1';
  const img = document.createElement('img');
  img.src = `${window.location.origin}${watermarkSrc}`;
  img.alt = '';
  img.crossOrigin = 'anonymous';
  img.style.cssText = 'width:520px;max-width:78%;height:auto;object-fit:contain;';
  watermark.appendChild(img);
  page.appendChild(watermark);

  const content = document.createElement('div');
  content.className = 'proposal-pdf-page-content';
  content.style.cssText = 'position:relative;z-index:1;width:100%;height:auto;min-height:0;overflow:visible';
  page.appendChild(content);
  return page;
}

function getPageContent(page: HTMLDivElement): HTMLDivElement {
  return page.querySelector('.proposal-pdf-page-content') as HTMLDivElement;
}

function contentScrollHeight(content: HTMLElement): number {
  return Math.ceil(content.scrollHeight);
}

async function waitForFonts() {
  try {
    await document.fonts.ready;
  } catch {
    /* ignore */
  }
}

async function waitForImages(root: HTMLElement) {
  const images = Array.from(root.querySelectorAll('img'));
  await Promise.all(
    images.map(
      (img) =>
        new Promise<void>((resolve) => {
          if (img.complete && img.naturalWidth > 0) {
            resolve();
            return;
          }
          img.onload = () => resolve();
          img.onerror = () => resolve();
          setTimeout(() => resolve(), 2000);
        })
    )
  );
}

export async function exportProposalPdf(
  sourceEl: HTMLElement,
  fileName: string,
  watermarkSrc = QUOTE_WATERMARK
) {
  const [{ default: html2canvas }, { jsPDF }] = await Promise.all([
    import('html2canvas-pro'),
    import('jspdf')
  ]);

  const host = document.createElement('div');
  host.style.cssText = `position:fixed;left:-12000px;top:0;width:${PAGE_WIDTH_PX}px;background:#fff;z-index:-1;pointer-events:none`;
  document.body.appendChild(host);

  try {
    const clone = sourceEl.cloneNode(true) as HTMLElement;
    clone.style.width = `${PAGE_WIDTH_PX}px`;
    clone.style.maxWidth = `${PAGE_WIDTH_PX}px`;
    clone.style.boxShadow = 'none';
    clone.style.margin = '0';
    clone.querySelectorAll('[data-pdf-watermark]').forEach((node) => node.remove());

    const measureWrap = document.createElement('div');
    measureWrap.style.cssText = `width:${PAGE_WIDTH_PX}px;padding:0;box-sizing:border-box`;
    measureWrap.appendChild(clone);
    host.appendChild(measureWrap);
    await waitForFonts();
    await waitForImages(clone);

    const allBlocks = Array.from(clone.querySelectorAll<HTMLElement>('[data-pdf-block]'));
    const blocks = allBlocks.filter((el) => !el.parentElement?.closest('[data-pdf-block]'));
    if (!blocks.length) {
      throw new Error('No printable blocks found in preview.');
    }

    const pages: HTMLDivElement[] = [];
    let currentPage = createPageShell(watermarkSrc);
    let currentContent = getPageContent(currentPage);
    host.appendChild(currentPage);
    pages.push(currentPage);

    for (const block of blocks) {
      currentContent.appendChild(block);
      void currentContent.offsetHeight;
      if (contentScrollHeight(currentContent) <= MAX_CONTENT_HEIGHT + 1) {
        continue;
      }
      currentContent.removeChild(block);
      if (currentContent.childElementCount > 0) {
        currentPage = createPageShell(watermarkSrc);
        currentContent = getPageContent(currentPage);
        host.appendChild(currentPage);
        pages.push(currentPage);
      }
      currentContent.appendChild(block);
    }

    while (pages.length > 1 && getPageContent(pages[pages.length - 1]).childElementCount === 0) {
      pages.pop()?.remove();
    }

    measureWrap.remove();
    await waitForFonts();
    await waitForImages(host);

    const pdf = new jsPDF({ orientation: 'portrait', unit: 'mm', format: 'a4' });
    const pageWidth = pdf.internal.pageSize.getWidth();
    const pageHeight = pdf.internal.pageSize.getHeight();

    for (let i = 0; i < pages.length; i++) {
      const page = pages[i];
      page.style.height = `${PAGE_HEIGHT_PX}px`;
      page.style.overflow = 'hidden';
      const canvas = await html2canvas(page, {
        scale: 2,
        useCORS: true,
        allowTaint: true,
        backgroundColor: '#ffffff',
        logging: false,
        width: PAGE_WIDTH_PX,
        height: PAGE_HEIGHT_PX,
        windowWidth: 1200,
        windowHeight: PAGE_HEIGHT_PX,
        onclone: (clonedDoc) => {
          clonedDoc.querySelectorAll('h3, .sheet-kicker').forEach((el) => {
            (el as HTMLElement).style.fontFamily = "'Cormorant Garamond', Georgia, serif";
            (el as HTMLElement).style.fontWeight = '600';
          });
          clonedDoc.querySelectorAll('.proposal-pdf-page, .proposal-sheet, .invoice-sheet').forEach((el) => {
            (el as HTMLElement).style.fontFamily = "'Source Sans 3', 'Segoe UI', sans-serif";
          });
        }
      });
      const imgData = canvas.toDataURL('image/jpeg', 0.96);
      if (i > 0) pdf.addPage();
      pdf.addImage(imgData, 'JPEG', 0, 0, pageWidth, pageHeight);
    }

    pdf.save(fileName);
  } finally {
    host.remove();
  }
}
