Ext.define('WEBAPP.model.BA0006M', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'XACTION', type: 'string' },
        { name: 'TX_QTY_T', type: 'string' },
        { name: 'WH_NO', type: 'string' },
        { name: 'PO_NO', type: 'string' },
        { name: 'MMCODE', type: 'string' },
        { name: 'MAT_CLASS', type: 'string' },
        { name: 'SEQ', type: 'string' },
        { name: 'MMNAME_C', type: 'string' },
        { name: 'MMNAME_E', type: 'string' },
        { name: 'M_STOREID', type: 'string' },
        { name: 'ACC_QTY', type: 'string' },  // 入帳數量=進貨數量*UNIT_SWAP
        { name: 'ACC_BASEUNIT', type: 'string' },
        { name: 'ACC_PURUN', type: 'string' },
        { name: 'BW_SQTY', type: 'string' },
        { name: 'LOT_NO', type: 'string' },
        { name: 'EXP_DATE', type: 'string' },
        { name: 'STATUS', type: 'string' },
        { name: 'ACC_USER', type: 'string' },
        { name: 'ACC_TIME', type: 'string' }, 
        { name: 'UPRICE', type: 'string' },
        { name: 'AGEN_NAMEC', type: 'string' }, 
        { name: 'AGEN_NO', type: 'string' },
        { name: 'M_CONTID', type: 'string' },
        { name: 'M_CONTPRICE', type: 'string' },
        { name: 'APL_INQTY', type: 'string' },
        { name: 'UPRICE', type: 'string' },
        { name: 'M_PURUN', type: 'string' },
        { name: 'UNIT_SWAP', type: 'string' },
        { name: 'INV_QTY', type: 'string' },
        { name: 'M_DISCPERC', type: 'string' }, //折讓
        { name: 'DISC_CPRICE', type: 'string' }, //優惠價
        { name: 'INQTY', type: 'string' },      //進貨數量 
        { name: 'BASE_UNIT', type: 'string' },    //最小計量單位 
        { name: 'WEXP_ID', type: 'string' },   //批號追蹤
        { name: 'MEMO', type: 'string' },
        { name: 'INID', type: 'string' },
        { name: 'DISC_UPRICE', type: 'string' }
    ]
});
