Ext.define('WEBAPP.model.CHK_CE0019VM', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'MMCODE', type: 'string' },
        { name: 'MMNAME_C', type: 'string' },
        { name: 'MMNAME_E', type: 'string' },
        { name: 'BASE_UNIT', type: 'string' }, 
        { name: 'CHK_QTY', type: 'string' }, 
        { name: 'CHK_UID_NAME', type: 'string' }, 
        { name: 'CHK_TIME', type: 'date' }, 
        { name: 'UPDN_STATUS', type: 'string' }, 

        { name: 'CHK_YM', type: 'string' },
        { name: 'M_CONTPRICE', type: 'string' },
        { name: 'M_CONTPPRICE', type: 'string' },
        { name: 'E_TAKEKIND', type: 'string' },
        { name: 'CREATE_DATE', type: 'string' },
        { name: 'CREATE_USER', type: 'string' },
        { name: 'UPDATE_DATE', type: 'string' },
        { name: 'UPDATE_IP', type: 'string' },
        { name: 'UPDATE_USER', type: 'string' },
    ]
});