Ext.define('WEBAPP.model.BackStorageDt', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'DOCNO', type: 'string' },
        { name: 'SEQ', type: 'string' },
        { name: 'FLOW_ID', type: 'string' },        
        { name: 'MAT_CLSNAME', type: 'string' },
        { name: 'APPDEPT', type: 'string' },
        { name: 'APPTIME', type: 'string' },
        { name: 'APPLY_NOTE', type: 'string' },
        { name: 'STAT', type: 'string' },
        { name: 'MMCODE', type: 'string' },
        { name: 'MMNAME_C', type: 'string' },
        { name: 'MMNAME_E', type: 'string' },
        { name: 'APVQTY', type: 'string' },
        { name: 'BASE_UNIT', type: 'string' },
        { name: 'AVG_PRICE', type: 'string' }
    ]

});