(() => {
    'use strict';
    const form = document.getElementById('payment-form');
    if (!form) return;
    const rows = document.getElementById('allocation-rows');
    const picker = document.getElementById('eligible-picker');
    const confirmation = form.querySelector('[name="Input.FullAmountsConfirmed"]');
    const message = document.getElementById('payment-message');
    const options = new Map(JSON.parse(document.getElementById('payment-options').textContent).map(p => [String(p.id), p]));
    let candidates = [], selectionVersion = 0, totalVersion = 0, timer;
    const field = (row, name) => row.querySelector(`[name$=".${name}"]`);
    const allRows = () => [...rows.querySelectorAll('.allocation-row')];
    const showError = text => { message.hidden = false; message.textContent = text; };
    const resetConfirmation = () => { confirmation.checked = false; };
    const info = p => `قرارداد ${p.contractNo} — ${p.topic} | صورت وضعیت ${p.paymentNo}: ${p.title} (${p.paymentType}) | تأییدشده: ${p.approvedAmount} | تخصیص قبلی: ${p.totalAllocated} | مانده قابل تخصیص: ${p.remainingAmount} ریال`;
    const post = async (url, data) => {
        data.set('__RequestVerificationToken', form.querySelector('[name="__RequestVerificationToken"]').value);
        const response = await fetch(url, { method: 'POST', body: data });
        let result;
        try { result = await response.json(); } catch { throw new Error('پاسخ سرور قابل دریافت نیست. دوباره تلاش کنید.'); }
        if (!response.ok) throw new Error(result.errorCode === 'Concurrency'
            ? 'سند تغییر کرده است. اطلاعات واردشده حفظ شده؛ نسخه جدید را بارگذاری و دوباره بررسی کنید.'
            : 'عملیات انجام نشد: ' + (result.message || 'مقادیر ورودی و اتصال را بررسی کنید.'));
        return result;
    };
    function reindex() {
        allRows().forEach((row, i) => {
            row.querySelectorAll('[name]').forEach(el => { el.name = el.name.replace(/Input\.Allocations\[[^\]]+\]/, `Input.Allocations[${i}]`); });
        });
    }
    function refreshPicker() {
        const selected = new Set(allRows().map(r => field(r, 'ProgressPaymentId').value));
        picker.replaceChildren(new Option('انتخاب کنید', ''));
        candidates.forEach(p => { const option = new Option(info(p), String(p.id)); option.disabled = selected.has(String(p.id)); picker.add(option); });
        document.getElementById('eligible-info').textContent = candidates.length ? 'ردیف‌های اضافه‌شده دوباره قابل انتخاب نیستند.' : 'صورت وضعیت واجد شرایطی در قراردادهای انتخاب‌شده وجود ندارد.';
    }
    async function loadEligible() {
        const version = ++selectionVersion;
        const query = new URLSearchParams();
        form.querySelectorAll('[name="Input.SelectedContractIds"]:checked').forEach(el => query.append('contractIds', el.value));
        picker.disabled = true;
        try {
            const response = await fetch(form.dataset.eligibleUrl + '?' + query);
            if (!response.ok) throw new Error('فهرست صورت وضعیت‌ها دریافت نشد. انتخاب قرارداد را دوباره انجام دهید.');
            const result = await response.json();
            if (version !== selectionVersion) return;
            candidates = result; result.forEach(p => { if (!options.has(String(p.id))) options.set(String(p.id), p); });
            refreshPicker();
        } catch (e) { if (version === selectionVersion) { candidates = []; refreshPicker(); showError(e.message); } }
        finally { if (version === selectionVersion) picker.disabled = false; }
    }
    function scheduleTotals() {
        const version = ++totalVersion;
        clearTimeout(timer);
        document.getElementById('document-totals').textContent = 'در حال محاسبه…';
        timer = setTimeout(async () => {
            const data = new FormData();
            data.set('totalAmount', form.querySelector('[name="Input.TotalAmount"]').value);
            allRows().forEach(r => data.append('allocatedAmounts', field(r, 'AllocatedAmount').value));
            try {
                const result = await post(form.dataset.totalsUrl, data);
                if (version === totalVersion) document.getElementById('document-totals').textContent = `مبلغ سند: ${result.totalAmount} | تخصیص‌یافته: ${result.totalAllocated} | تخصیص‌نیافته: ${result.unallocatedAmount}`;
            } catch (e) { if (version === totalVersion) document.getElementById('document-totals').textContent = e.message; }
        }, 250);
    }
    async function preview(row) {
        const version = (row.previewVersion || 0) + 1; row.previewVersion = version;
        field(row, 'ReselectFull').value = 'true'; field(row, 'ReviewToken').value = '';
        resetConfirmation();
        const status = row.querySelector('.row-status'); status.textContent = 'در حال دریافت پیش‌نمایش کامل…';
        const data = new FormData(); data.set('progressPaymentId', field(row, 'ProgressPaymentId').value); data.set('allocationType', '1');
        if (form.dataset.documentId) { data.set('documentId', form.dataset.documentId); data.set('rowVersion', form.querySelector('[name="Input.RowVersion"]').value); }
        try {
            const result = await post(form.dataset.previewUrl, data);
            if (!row.isConnected || version !== row.previewVersion || field(row, 'AllocationType').value !== '1') return;
            field(row, 'AllocatedAmount').value = result.allocatedAmount;
            field(row, 'ReviewToken').value = result.reviewToken;
            status.textContent = `پیش‌نمایش کامل: ${result.allocatedAmount} ریال. تأیید صریح پیش از ذخیره لازم است.`;
            resetConfirmation(); scheduleTotals();
        } catch (e) { if (version === row.previewVersion) { status.textContent = e.message; resetConfirmation(); } }
    }
    function initialize(row) {
        const p = options.get(field(row, 'ProgressPaymentId').value);
        if (p) { row.dataset.contractId = String(p.contractId); row.querySelector('.row-title').textContent = `قرارداد ${p.contractNo} — صورت وضعیت ${p.paymentNo}`; row.querySelector('.row-info').textContent = info(p); }
        const full = field(row, 'AllocationType').value === '1';
        field(row, 'AllocatedAmount').readOnly = full;
        row.querySelector('.preview-full').hidden = !full;
        row.querySelector('.row-status').textContent = full ? (field(row, 'ReviewToken').value ? 'پیش‌نمایش کامل آماده بررسی و تأیید است.' : field(row, 'Id').value !== '0' && field(row, 'ReselectFull').value.toLowerCase() !== 'true' ? 'مبلغ کامل ذخیره‌شده؛ بدون محاسبه مجدد حفظ می‌شود.' : 'برای دریافت مبلغ کامل، پیش‌نمایش را درخواست کنید.') : 'مبلغ جزئی را تا سقف مانده قابل تخصیص وارد کنید.';
        row.querySelector('.remove-row').addEventListener('click', () => { row.remove(); reindex(); resetConfirmation(); refreshPicker(); scheduleTotals(); });
        row.querySelector('.preview-full').addEventListener('click', () => preview(row));
        field(row, 'AllocationType').addEventListener('change', () => {
            row.previewVersion = (row.previewVersion || 0) + 1;
            const full = field(row, 'AllocationType').value === '1';
            field(row, 'AllocatedAmount').readOnly = full; row.querySelector('.preview-full').hidden = !full;
            field(row, 'ReviewToken').value = ''; field(row, 'ReselectFull').value = full ? 'true' : 'false';
            resetConfirmation(); if (full) preview(row); else row.querySelector('.row-status').textContent = 'مبلغ جزئی را وارد کنید.';
            scheduleTotals();
        });
    }
    allRows().forEach(initialize);
    const contractOrder = [...new Set(allRows().map(r => r.dataset.contractId))];
    contractOrder.forEach(id => allRows().filter(r => r.dataset.contractId === id).forEach(r => rows.append(r)));
    reindex();
    document.getElementById('add-allocation').addEventListener('click', () => {
        const p = options.get(picker.value); if (!p || allRows().some(r => field(r, 'ProgressPaymentId').value === picker.value)) return;
        const fragment = document.getElementById('allocation-template').content.cloneNode(true);
        const row = fragment.querySelector('.allocation-row');
        const unique = 'new-' + Date.now();
        row.querySelectorAll('[id]').forEach(el => { el.id = el.id.replace('__index__', unique); });
        row.querySelectorAll('[for]').forEach(el => { el.htmlFor = el.htmlFor.replace('__index__', unique); });
        field(row, 'ProgressPaymentId').value = String(p.id);
        // Group visually by contract without changing any allocation amounts.
        const siblings = allRows().filter(r => r.dataset.contractId === String(p.contractId));
        if (siblings.length) siblings[siblings.length - 1].after(row); else rows.append(row);
        reindex(); initialize(row); refreshPicker(); resetConfirmation(); preview(row); scheduleTotals();
    });
    form.querySelectorAll('[name="Input.SelectedContractIds"]').forEach(el => el.addEventListener('change', loadEligible));
    document.getElementById('contract-search').addEventListener('input', e => {
        form.querySelectorAll('.contract-option').forEach(el => { el.classList.toggle('d-none', !el.textContent.includes(e.target.value)); });
    });
    form.addEventListener('input', e => { if (e.target.matches('.allocation-amount, [name="Input.TotalAmount"]')) scheduleTotals(); });
    form.addEventListener('submit', e => {
        reindex();
        const needsReview = allRows().filter(r => field(r, 'AllocationType').value === '1' && (field(r, 'Id').value === '0' || field(r, 'ReselectFull').value.toLowerCase() === 'true' || field(r, 'ReviewToken').value));
        if (!allRows().length || needsReview.some(r => !field(r, 'ReviewToken').value) || (needsReview.length && !confirmation.checked)) {
            e.preventDefault(); showError('حداقل یک تخصیص لازم است. برای مبالغ کامل جدید، پیش‌نمایش دریافت و سپس تأیید کنید.'); message.scrollIntoView({ block: 'center' });
        }
    });
    loadEligible(); scheduleTotals();
})();
