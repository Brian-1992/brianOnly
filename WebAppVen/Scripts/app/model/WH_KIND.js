Ext.define('WEBAPP.model.WH_KIND', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'GRP_CODE', type: 'string' },        // 群組代碼
        { name: 'DATA_SEQ', type: 'string' },        // SEQ NO
        { name: 'DATA_NAME', type: 'string' },       // 庫房名稱
        { name: 'DATA_VALUE',type: 'string' },       // 庫房代碼(1庫 2局(衛星庫) 3病房 4科室 5戰備庫)
        { name: 'DATA_DESC', type: 'string' },       // 庫房說明
        { name: 'DATA_FLAG', type: 'string' },       // DATA FLAG
        { name: 'DATA_REMARK', type: 'string' }
    ]
});