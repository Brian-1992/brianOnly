Ext.define('WEBAPP.model.PH_REPLY', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'AGEN_NO', type: 'string' },
        { name: 'DNO', type: 'string' },
        { name: 'MMCODE', type: 'string' },
        { name: 'PO_NO', type: 'string' },
        { name: 'SEQ', type: 'string' },
        { name: 'BW_SQTY', type: 'string' },
        { name: 'ACEPT_QTY', type: 'string' },
        { name: 'MEMO', type: 'string' },
        { name: 'STATUS', type: 'string' },
        { name: 'CREATE_TIME', type: 'string' },
        { name: 'CREATE_USER', type: 'string' },
        { name: 'UPDATE_TIME', type: 'string' },
        { name: 'UPDATE_USER', type: 'string' },
        { name: 'UPDATE_IP', type: 'string' },
        // =========================================
        { name: 'PO_TYPE', type: 'string' }
    ]
});