Ext.define('WEBAPP.model.MM_PO_M', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'PO_NO', type: 'string' },
        { name: 'AGEN_NO', type: 'string' },
        { name: 'PO_TIME', type: 'date' },
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
        { name: 'ISCOPY', type: 'string' }
    ]
});