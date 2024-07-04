Ext.define('WEBAPP.model.ME_DOCD', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'DOCNO', type: 'string' },
        { name: 'SEQ', type: 'string' },
        { name: 'MMCODE', type: 'string' },
        { name: 'APPQTY', type: 'string' },
        { name: 'APVQTY', type: 'string' },
        { name: 'APVTIME', type: 'string' },
        { name: 'APVID', type: 'string' },
        { name: 'ACKQTY', type: 'string' },
        { name: 'ACKID', type: 'string' },
        { name: 'ACKTIME', type: 'string' },
        { name: 'STAT', type: 'string' },
        { name: 'RSEQ', type: 'string' },
        { name: 'EXPT_DISTQTY', type: 'string' },
        { name: 'PICK_QTY', type: 'string' },
        { name: 'PICK_USER', type: 'string' },
        { name: 'PICK_TIME', type: 'string' }, 
        { name: 'ONWAY_QTY', type: 'string' },
        { name: 'APLYITEM_NOTE', type: 'string' },
        { name: 'AMT', type: 'string' },
        { name: 'UP', type: 'string' },
        { name: 'CREATE_TIME', type: 'string' },
        { name: 'CREATE_USER', type: 'string' },
        { name: 'UPDATE_TIME', type: 'string' },
        { name: 'UPDATE_USER', type: 'string' },
        { name: 'UPDATE_IP', type: 'string' },
        { name: 'MMNAME_C', type: 'string' },
        { name: 'MMNAME_E', type: 'string' },
        { name: 'BASE_UNIT', type: 'string' },
        { name: 'M_CONTPRICE', type: 'string' },
        { name: 'AVG_PRICE', type: 'string' },
        { name: 'INV_QTY', type: 'string' },
        { name: 'AVG_APLQTY', type: 'string' },

        { name: 'SAFE_QTY', type: 'string' }, 
        { name: 'BALANCE', type: 'string' },
        { name: 'LOT_NO', type: 'string' },
        { name: 'LOT_NO_N', type: 'string' },
        { name: 'EXPDATE', type: 'string' },
        { name: 'EXPDATE_T', type: 'string' },
        { name: 'POSTID', type: 'string' },
        { name: 'APPDEPT', type: 'string' },
        { name: 'DRUGMEMO', type: 'string' },
        { name: 'LOW_QTY', type: 'string' },

        // =================== 湘倫 ======================
        { name: 'AMOUNT', type: 'string' },
        // ===============================================

        { name: 'WEXP_ID', type: 'string' },
        { name: 'WEXP_ID_DESC', type: 'string' },
        { name: 'FLOWID', type: 'string' },
        { name: 'SRCDOCNO', type: 'string' }

    ]
});