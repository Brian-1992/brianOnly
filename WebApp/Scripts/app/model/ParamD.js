Ext.define('WEBAPP.model.ParamD', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'GRP_CODE', type: 'string' },
        { name: 'DATA_SEQ', type: 'int' },
        { name: 'DATA_NAME', type: 'string' },
        { name: 'DATA_VALUE', type: 'string' },
        { name: 'DATA_DESC', type: 'string' },
        { name: 'DATA_REMARK', type: 'string' }
    ]
});