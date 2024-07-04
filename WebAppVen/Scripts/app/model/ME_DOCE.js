Ext.define('WEBAPP.model.ME_DOCE', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'DOCNO', type: 'string' },
        { name: 'SEQ', type: 'string' },
        { name: 'EXPDATE', type: 'date' },
        { name: 'MMCODE', type: 'string' },
        { name: 'APVQTY', type: 'string' },
        { name: 'APVTIME', type: 'string' },
        { name: 'CREATE_USER', type: 'string' },
        { name: 'UPDATE_DATE', type: 'string' },
        { name: 'UPDATE_USER', type: 'string' },
        { name: 'UPDATE_IP', type: 'string' },
        { name: 'MMNAME_C', type: 'string' },
        { name: 'MMNAME_E', type: 'string' }
    ]
});