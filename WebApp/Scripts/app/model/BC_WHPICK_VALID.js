Ext.define('WEBAPP.model.BC_WHPICK_VALID', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'WH_NO', type: 'string' },
        { name: 'PICK_DATE', type: 'string' },
        { name: 'DOCNO', type: 'string' },
        { name: 'SEQ', type: 'string' },
        { name: 'VALID_DATE', type: 'string' },
        { name: 'ACT_PICK_QTY', type: 'string' },
        { name: 'CREATE_DATE', type: 'string' },
        { name: 'CREATE_USER', type: 'string' },
        { name: 'UPDATE_DATE', type: 'string' },
        { name: 'UPDATE_USER', type: 'string' },
        { name: 'UPDATE_IP', type: 'string' },
    ]
});