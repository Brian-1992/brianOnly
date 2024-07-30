/** */
Ext.define('MMIS.form.CRUDForm', {
    extend: 'MMIS.form.Form',

    requires: ['MMIS.Utility'],

    mixins: ['MMIS.data.TaskFlowHelper'],

    statics: {
        /** */
        STATE_READ: 'R',

        /** */
        STATE_INSERT: 'I',

        /** */
        STATE_UPDATE: 'U',

        /** */
        STATE_DELETE: 'D'
    },

    config: {
        formState: 'R'
    },

    saveText: '儲存',
    cancelText: '取消',

    /** @Config {String} taskFlow */
    taskFlow: null,

    saveButton: null,

    cancelButton: null,

    defaultButton: true,

    border: false,

    autoScroll: true,

    trackResetOnLoad: true,

    initComponent: function () {
        this.callParent(arguments);
    },

    beforeInitComponent: function () {
        var me = this;

        me.callParent(arguments);

        /**
         * @event beforeSubmit
         * Fires before submit
         */

        /**
         * @event cancel
         */

        /**
         * @event statechange
         * @param {String} new state
         */
        me.addEvents('beforeSubmit', 'cancel', 'statechange');

        Ext.apply(me, Ext.applyIf(me.initialConfig, { state: me.STATE_READ }));

        if (me.defaultButton) {
            me.saveButton = me.generateSaveButton();
            me.cancelButton = me.generateCancelButton();
            me.buttons = [me.saveButton, me.cancelButton];
        }
    },

    afterInitComponent: function () {
        var me = this;
        me.setFormState(MMIS.form.CRUDForm.STATE_READ);
        me.preventAccidentallyLeave();
        me.callParent(arguments);
    },

    preventAccidentallyLeave: function () {
        var me = this;

        me.getForm().getFields().each(function (field) {
            Ext.override(field, {
                setValue: function (value) {
                    var rtn = this.callParent(arguments);
                    this.resetOriginalValue();
                }
            });
        });

        window.onbeforeunload = function () {
            if (me.isDirty()) {
                return "您的資料可能尚未儲存，確定要離開此頁面？";
            }
        };
    },

    generateSaveButton: function () {
        var me = this;
        return Ext.widget("button", {
            text: me.saveText,
            scope: me,
            handler: me.saveButtonHandler
        });
    },

    generateCancelButton: function () {
        var me = this;
        return Ext.widget("button", {
            text: me.cancelText,
            scope: me,
            handler: me.cancelButtonHandler
        });
    },

    saveButtonHandler: function () {
        this.confirmSubmit();
    },

    cancelButtonHandler: function () {
        var me = this;
        me.getForm().reset();
        me.setFormState(MMIS.form.CRUDForm.STATE_READ);
        me.fireEvent("cancel");
    },

    getActionText: function (state) {
        var myClass = MMIS.form.CRUDForm;
        switch (state) {
            case myClass.STATE_INSERT:
                return "新增";
            case myClass.STATE_UPDATE:
                return "修編";
            case myClass.STATE_DELETE:
                return "刪除";
        }
    },

    showClientInvalidWarn: function () {
        Ext.Msg.alert('提醒', 'W0021:輸入資料格式有誤');
        MMIS.Utility.msglabel('W0021:輸入資料格式有誤');
    },

    confirmSubmit: function () {
        var me = this,
            actionText = me.getActionText(me.formState);

        me.showConfirmDialog(actionText, me.submit);
    },

    showConfirmDialog: function (actionText, callBack) {
        Ext.Msg.confirm(actionText, '確定' + actionText + '?', function (btnId, text) {
            if (btnId === 'yes') {
                callBack.call(this);
            }
        }, this);
    },

    submit: function () {
        var me = this;

        me.fireEvent("beforeSubmit");
        me.getForm().submit({
            url: me.parseTaskFlow(me.taskFlow),
            params: { x: me.formState },
            success: function (form, action) {
                me.fireEvent("success", form, action);
            },
            failure: function (form, action) {
                me.fireEvent("failure", form, action);
                switch (action.failureType) {
                    case Ext.form.action.Action.CLIENT_INVALID:
                        me.showClientInvalidWarn();
                        break;
                    case Ext.form.action.Action.CONNECT_FAILURE:
                        Ext.Msg.alert('失敗', 'Ajax communication failed');
                        break;
                    case Ext.form.action.Action.SERVER_INVALID:
                        Ext.Msg.alert('失敗', action.result.msg);
                        break;
                }
            }
        });
    },

    applyFormState: function (state) {
        var me = this;
        switch (state) {
            case MMIS.form.CRUDForm.STATE_READ:
                me.setButtonsVisible(false);
                me.setReadOnly(true);
                break;
            case MMIS.form.CRUDForm.STATE_INSERT:
                me.setButtonsVisible(true);
                me.setReadOnly(false);
                break;
            case MMIS.form.CRUDForm.STATE_UPDATE:
                me.setButtonsVisible(true);
                me.setReadOnlyForUpdate();
                break;
            case MMIS.form.CRUDForm.STATE_DELETE:
                break;
        }
        me.fireEvent("statechange", state);
        return state;
    },

    setReadOnly: function (b) {
        this.form.getFields().each(function (field) {
            if (field.initialConfig.readOnly) {
                return;
            }
            field.setReadOnly(b);
        });
    },

    setReadOnlyForUpdate: function () {
        this.form.getFields().each(function (field) {
            if (field.initialConfig.readOnly || field.initialConfig.insertOnly) {
                return;
            }
            field.setReadOnly(false);
        });
    },

    setButtonsVisible: function (b) {
        var me = this;
        me.saveButton.setVisible(b);
        me.cancelButton.setVisible(b);
    },

    clear: function () {
        var me = this;
        if (me.model) {
            me.loadRecord(Ext.create(me.model));
        } else {
            me.getForm().getFields().each(function (field) {
                field.setValue('');
            });
        }
        me.getForm().clearInvalid();
    }
});