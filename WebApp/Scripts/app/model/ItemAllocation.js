Ext.define('WEBAPP.model.ItemAllocation', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'MMCODE', type: 'string' },
        { name: 'MMNAME_C', type: 'string' },
        { name: 'MMNAME_E', type: 'string' },
        { name: 'MAT_CLSNAME', type: 'string' },
        { name: 'BASE_UNIT', type: 'string' },
        { name: 'INID_NAME_USER', type: 'string' },
        { name: 'APPDEPT', type: 'string' },
        { name: 'INID', type: 'string' },
        { name: 'INID_NAME', type: 'string' },
        { name: 'APPTIME_YM', type: 'string' },
        { name: 'M_STOREID', type: 'string' },
        { name: 'APVQTYN', type: 'string' },
        { name: 'AVG_PRICE', type: 'string' },
        { name: 'M_CONTPRICE', type: 'string' },
        { name: 'M_ALLPRICE', type: 'string' },
        { name: 'TOWH', type: 'string' },
        { name: 'WH_NAME', type: 'string' }
    ]
});