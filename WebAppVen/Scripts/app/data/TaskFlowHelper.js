/** */
Ext.define('MMIS.data.TaskFlowHelper', {
    parseTaskFlow: function (str) {
        var prefix = '../../../api/flow/process/';
        return str.indexOf('/') !== -1 ? str : prefix + str;
    }
});
