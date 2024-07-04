Ext.define('WEBAPP.model.BG0006M', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'PO_NO', type: 'string' },		//訂單號碼
        { name: 'MMCODE', type: 'string' },		//院內碼
        { name: 'PO_QTY', type: 'string' },		//訂單數量(包裝單位數量)
        { name: 'PO_PRICE', type: 'string' },		//訂單單價(合約單價)
        { name: 'M_PURUN', type: 'string' },		//申購計量單位
        { name: 'M_AGENLAB', type: 'string' },		//廠牌
        { name: 'PO_AMT', type: 'string' },		//總金額
        { name: 'M_DISCPERC', type: 'string' },		//折讓比
        { name: 'DELI_QTY', type: 'string' },		//已交數量
        { name: 'BW_SQTY', type: 'string' },		//借貨數量
        { name: 'DELI_STATUS', type: 'string' },		//交貨狀態 C-已交貨
        { name: 'CREATE_TIME', type: 'string' },		//建立時間
        { name: 'CREATE_USER', type: 'string' },		//建立人員代碼
        { name: 'UPDATE_TIME', type: 'string' },		//異動時間
        { name: 'UPDATE_USER', type: 'string' },		//異動人員代碼
        { name: 'UPDATE_IP', type: 'string' },		//異動IP
        { name: 'MEMO', type: 'string' },		//備註
        { name: 'PR_NO', type: 'string' },		//申購單號
        { name: 'UNIT_SWAP', type: 'string' },		//轉換率
        { name: 'INVOICE', type: 'string' },		//發票號碼
        { name: 'INVOICE_DT', type: 'string' },		//發票號碼日期
        { name: 'CKIN_QTY', type: 'string' },		//發票驗證數量
        { name: 'CHK_USER', type: 'string' },		//發票驗證人員
        { name: 'CHK_DT', type: 'string' },		//發票驗證日期
        { name: 'ACCOUNTDATE', type: 'string' },		//入帳日期
        { name: 'STATUS', type: 'string' },		//default 'N' , D-作廢,    N-申購
        { name: 'UPRICE', type: 'string' },		//最小單價(計量單位單價)
        { name: 'DISC_CPRICE', type: 'string' },		//優惠合約單價
        { name: 'DISC_UPRICE', type: 'string' },		//優惠最小單價(計量單位單價)
        { name: 'CREATE_TIME', type: 'string' },
        { name: 'CREATE_USER', type: 'string' },
        { name: 'UPDATE_TIME', type: 'string' },
        { name: 'UPDATE_USER', type: 'string' },
        { name: 'UPDATE_IP', type: 'string' },
        { name: 'PRICE_AMOUNT', type: 'string' },        //申購金額
        { name: 'ACC_TIME', type: 'string' },        //進貨日期
        { name: 'ACC_QTY', type: 'string' },     //數量
        { name: 'LOT_NO', type: 'string' },     //批號
        { name: 'EXP_DATE', type: 'string' }     //效期
    ]
});