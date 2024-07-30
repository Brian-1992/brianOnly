Ext.define('WEBAPP.model.WB_MM_PO_M', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'PO_NO', type: 'string' },
        { name: 'AGEN_NO', type: 'string' },
        { name: 'PO_TIME', type: 'string' },
        { name: 'M_CONTID', type: 'string' },
        { name: 'PO_STATUS', type: 'string' },
        { name: 'CREATE_TIME', type: 'string' },
        { name: 'CREATE_USER', type: 'string' },
        { name: 'UPDATE_TIME', type: 'string' },
        { name: 'UPDATE_USER', type: 'string' },
        { name: 'UPDATE_IP', type: 'string' },
        { name: 'MEMO', type: 'string' },
        { name: 'ISCONFIRM', type: 'string' },
        { name: 'ISBACK', type: 'string' },
        { name: 'PHONE', type: 'string' },
        { name: 'SMEMO', type: 'string' },
        { name: 'ISCOPY', type: 'string' },

        { name: 'MMCODE', type: 'string' },
        { name: 'PO_QTY', type: 'string' },
        { name: 'PO_PRICE', type: 'string' },
        { name: 'M_PURUN', type: 'string' },
        { name: 'M_AGENLAB', type: 'string' },
        { name: 'PO_AMT', type: 'string' },
        { name: 'M_DISCPERC', type: 'string' },
        { name: 'UNIT_SWAP', type: 'string' },
        { name: 'MMNAME_C', type: 'string' },
        { name: 'MMNAME_E', type: 'string' }
    ]
});