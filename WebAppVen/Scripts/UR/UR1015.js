﻿Ext.onReady(function () {
    var T1Set = '../../../api/UR1002/Change';

    Ext.apply(Ext.form.field.VTypes, {
        paCheck: function (val) {
            var reg = /^(?=.*[0-9])(?=.*[a-zA-Z])([a-zA-Z0-9]+)$/;
            return reg.test(val);
        },
        paCheckText: '密碼必須是8至12碼數字或英文字母',
        paMatch: function (value, field) {
            var password = field.up('form').getForm().findField('NEW_PWD');
            return (value == password.getValue());
        },
        paMatchText: '確認新密碼與前次輸入不相符'
    });

    Ext.QuickTips.init();

    var T1Form = Ext.widget({
        xtype: 'form',
        layout: 'anchor',
        frame: false,
        bodyStyle: 'margin:5px;border:none',
        cls: 'T1b',
        autoScroll: true,
        width: 300,
        defaultType: 'textfield',
        fieldDefaults: {
            msgTarget: 'side',
            labelAlign: "right",
            labelWidth: 90
        },
        items: [{
            fieldLabel: '輸入舊密碼',
            name: 'OLD_PWD',
            inputType: 'password',
            enforceMaxLength: true,
            maxLength: 20,
            allowBlank: false,
            allowOnlyWhitespace: false,
            fieldCls: 'required'
        }, {
            fieldLabel: '輸入新密碼',
            name: 'NEW_PWD',
            inputType: 'password',
            vtype: 'paCheck',
            enforceMaxLength: true,
            minLength: 8,
            maxLength: 12,
            allowBlank: false,
            allowOnlyWhitespace: false,
            fieldCls: 'required'
        }, {
            fieldLabel: '確認新密碼',
            name: 'NEW_PWD_CHK',
            inputType: 'password',
            vtype: 'paMatch',
            enforceMaxLength: true,
            minLength: 8,
            maxLength: 12,
            allowBlank: false,
            allowOnlyWhitespace: false,
            fieldCls: 'required'
        }],
        buttons: [{
            itemId: 'submit', text: '儲存', handler: T1Submit
        }, {
            itemId: 'cancel', text: '清除', handler: T1Cleanup
        }]
    });

    function T1Submit()
    {
        var f = T1Form.getForm();
        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();
        f.submit({
            url: T1Set,
            success: function (form, action) {
                var f2 = T1Form.getForm();
                var r = f2.getRecord();
                if (action.result.afrs == 0)
                    Ext.Msg.alert('提示', '密碼變更失敗，請檢查舊密碼。');
                else
                    Ext.Msg.alert('完成', '密碼變更成功。');
                myMask.hide();
                T1Cleanup();
            },
            failure: function (form, action) {
                myMask.hide();
                switch (action.failureType) {
                    case Ext.form.action.Action.CLIENT_INVALID:
                        Ext.Msg.alert('錯誤', MMIS.Message.clientError);
                        break;
                    case Ext.form.action.Action.CONNECT_FAILURE:
                        Ext.Msg.alert('錯誤', MMIS.Message.communicationError);
                        break;
                    case Ext.form.action.Action.SERVER_INVALID:
                        Ext.Msg.alert('錯誤', action.result.msg);
                        break;
                }
            }
        });
    }

    function T1Cleanup() {
        T1Form.getForm().reset();
    }

    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'fit',
            width: 300,
            height: 300,
            padding: 0
        },
        items: [{
            itemId: 'form1panel',
            items: [T1Form]
        }
        ]
    });
});