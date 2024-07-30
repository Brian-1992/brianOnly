Ext.define('WEBAPP.model.MM_PR_DTBAS', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'MAT_CLSID', type: 'string' },
        { name: 'M_STOREID', type: 'string' },
        { name: 'DATEBAS', type: 'string' },
        { name: 'BEGINDATE', type: 'string' },
        { name: 'ENDDATE', type: 'string' },
        { name: 'SUMDATE', type: 'string' },
        { name: 'MTHBAS', type: 'string' },
        { name: 'LASTDELI_MTH', type: 'string' },
        { name: 'LASTDELI_DT', type: 'string' },
        { name: 'MMPRDTBAS_SEQ', type: 'string' },
        { name: 'CREATE_TIME', type: 'string' },
        { name: 'CREATE_USER', type: 'string' },
        { name: 'UPDATE_TIME', type: 'string' },
        { name: 'UPDATE_USER', type: 'string' }
    ]
});
