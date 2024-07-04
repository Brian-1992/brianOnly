Ext.Loader.loadScript({ url: location.pathname.substring(0, location.pathname.indexOf('Form')) + 'Scripts/TRAUtility.js' });
Ext.Loader.loadScript({ url: location.pathname.substring(0, location.pathname.indexOf('Form')) + 'Scripts/SP/SP01_detail_store.js' });
Ext.onReady(function () {
    Ext.QuickTips.init();

    function T1Load() {
        AccountStore.load({ params: { start: 0 } });
        fieldStore.load({ params: { start: 0 } });
        T1Store.load({ params: { start: 0 } });
        T2Store.load({ params: { start: 0 } });
        T3Store.load({ params: { start: 0 } });
        //T4Store.load({ params: { start: 0 } });
        T5Store.load({ params: { start: 0 } });
        T6Store.load({ params: { start: 0 } });
        T7Store.load({ params: { start: 0 } });
        T8Store.load({ params: { start: 0 } });
        T9Store.load({ params: { start: 0 } });
        T10Store.load({ params: { start: 0 } });
        T11Store.load({ params: { start: 0 } });
        T12Store.load({ params: { start: 0 } });
        //T13Store.load({ params: { start: 0 } });
        reloadFileList();
    }

    var topTool = Ext.create('Ext.toolbar.Toolbar', {
        border: 0,
        items: [
            {
                text: '瀏覽模式',
                id: 'viewMode',
                cls: 'funBtn',
                disabled: true,
                handler: function () {
                    Ext.getCmp('viewMode').setDisabled(true);
                    Ext.getCmp('editMode').setDisabled(false);
                    Ext.getCmp('NAME1').setFieldStyle('{font-weight:bold;color:black; border:0; background-color:#ecf5ff; background-image:none;}');
                    Ext.getCmp('NAME1').setReadOnly(true);
                    Ext.getCmp('NAME2').setFieldStyle('{font-weight:bold;color:black; border:0; background-color:#ecf5ff; background-image:none;}');
                    Ext.getCmp('NAME2').setReadOnly(true);
                    Ext.getCmp('TEL_NUMBER').setFieldStyle('{font-weight:bold;color:black; border:0; background-color:#ecf5ff; background-image:none;}');
                    Ext.getCmp('TEL_NUMBER').setReadOnly(true);
                    Ext.getCmp('FAX_NUMBER').setFieldStyle('{font-weight:bold;color:black; border:0; background-color:#ecf5ff; background-image:none;}');
                    Ext.getCmp('FAX_NUMBER').setReadOnly(true);
                    Ext.getCmp('SMTP_ADDR').setFieldStyle('{font-weight:bold;color:black; border:0; background-color:#ecf5ff; background-image:none;}');
                    Ext.getCmp('SMTP_ADDR').setReadOnly(true);
                    Ext.getCmp('URL').setFieldStyle('{font-weight:bold;color:black; border:0; background-color:#ecf5ff; background-image:none;}');
                    Ext.getCmp('URL').setReadOnly(true);
                    Ext.getCmp('ADDR').setFieldStyle('{font-weight:bold;color:black; border:0; background-color:#ecf5ff; background-image:none;}');
                    Ext.getCmp('ADDR').setReadOnly(true);
                    Ext.getCmp('POST_CODE1').setFieldStyle('{font-weight:bold;color:black; border:0; background-color:#ecf5ff; background-image:none;}');
                    Ext.getCmp('POST_CODE1').setReadOnly(true);
                    Ext.getCmp('J_1KFREPRE').setFieldStyle('{font-weight:bold;color:black; border:0; background-color:#ecf5ff; background-image:none;}');
                    Ext.getCmp('J_1KFREPRE').setReadOnly(true);
                    Ext.getCmp('ADDRC').setFieldStyle('{font-weight:bold;color:black; border:0; background-color:#ecf5ff; background-image:none;}');
                    Ext.getCmp('ADDRC').setReadOnly(true);
                    Ext.getCmp('ADDRS').setFieldStyle('{font-weight:bold;color:black; border:0; background-color:#ecf5ff; background-image:none;}');
                    Ext.getCmp('ADDRS').setReadOnly(true);
                    Ext.getCmp('CORPORATION').setFieldStyle('{font-weight:bold;color:black; border:0; background-color:#ecf5ff; background-image:none;}');
                    Ext.getCmp('CORPORATION').setReadOnly(true);
                    Ext.getCmp('btnSave').setVisible(false);
                    Ext.getCmp('btnSend').setVisible(false);
                    Ext.getCmp('btnAddKtokk').setVisible(false);
                    Ext.getCmp('btnCancel').setVisible(false);
                    
                    var grid = Ext.ComponentQuery.query('grid')[0];
                    var col = grid.getView().getHeaderCt().getHeaderAtIndex(1);
                    col.hide();
                }
            },
            {
                text: '編輯模式',
                id: 'editMode',
                cls: 'funBtn',
                handler: function () {
                    Ext.getCmp('editMode').setDisabled(true);
                    Ext.getCmp('viewMode').setDisabled(false);
                    Ext.getCmp('NAME1').setFieldStyle("{font-weight:normal; border:1px solid #b5b8c8; background-color:white; background-image:url('../../../Content/resources/images/form/text-bg.gif');}");
                    Ext.getCmp('NAME1').setReadOnly(false);
                    Ext.getCmp('NAME2').setFieldStyle("{font-weight:normal; border:1px solid #b5b8c8; background-color:white; background-image:url('../../../Content/resources/images/form/text-bg.gif');}");
                    Ext.getCmp('NAME2').setReadOnly(false);
                    Ext.getCmp('TEL_NUMBER').setFieldStyle("{font-weight:normal; border:1px solid #b5b8c8; background-color:white; background-image:url('../../../Content/resources/images/form/text-bg.gif');}");
                    Ext.getCmp('TEL_NUMBER').setReadOnly(false);
                    Ext.getCmp('FAX_NUMBER').setFieldStyle("{font-weight:normal; border:1px solid #b5b8c8; background-color:white; background-image:url('../../../Content/resources/images/form/text-bg.gif');}");
                    Ext.getCmp('FAX_NUMBER').setReadOnly(false);
                    Ext.getCmp('SMTP_ADDR').setFieldStyle("{font-weight:normal; border:1px solid #b5b8c8; background-color:white; background-image:url('../../../Content/resources/images/form/text-bg.gif');}");
                    Ext.getCmp('SMTP_ADDR').setReadOnly(false);
                    Ext.getCmp('URL').setFieldStyle("{font-weight:normal; border:1px solid #b5b8c8; background-color:white; background-image:url('../../../Content/resources/images/form/text-bg.gif');}");
                    Ext.getCmp('URL').setReadOnly(false);
                    Ext.getCmp('ADDR').setFieldStyle("{font-weight:normal; border:1px solid #b5b8c8; background-color:white; background-image:url('../../../Content/resources/images/form/text-bg.gif');}");
                    Ext.getCmp('ADDR').setReadOnly(false);
                    Ext.getCmp('POST_CODE1').setFieldStyle("{font-weight:normal; border:1px solid #b5b8c8; background-color:white; background-image:url('../../../Content/resources/images/form/text-bg.gif');}");
                    Ext.getCmp('POST_CODE1').setReadOnly(false);
                    Ext.getCmp('J_1KFREPRE').setFieldStyle("{font-weight:normal; border:1px solid #b5b8c8; background-color:white; background-image:url('../../../Content/resources/images/form/text-bg.gif');}");
                    Ext.getCmp('J_1KFREPRE').setReadOnly(false);
                    Ext.getCmp('ADDRC').setFieldStyle("{font-weight:normal; border:1px solid #b5b8c8; background-color:white; background-image:url('../../../Content/resources/images/form/text-bg.gif');}");
                    Ext.getCmp('ADDRC').setReadOnly(false);
                    Ext.getCmp('ADDRS').setFieldStyle("{font-weight:normal; border:1px solid #b5b8c8; background-color:white; background-image:url('../../../Content/resources/images/form/text-bg.gif');}");
                    Ext.getCmp('ADDRS').setReadOnly(false);
                    Ext.getCmp('CORPORATION').setFieldStyle("{font-weight:normal; border:1px solid #b5b8c8; background-color:white; background-image:url('../../../Content/resources/images/form/text-bg.gif');}");
                    Ext.getCmp('CORPORATION').setReadOnly(false);
                    //Ext.getCmp('btnSave').setVisible(true);
                    Ext.getCmp('btnSend').setVisible(true);
                    Ext.getCmp('btnAddKtokk').setVisible(true);
                    
                    if (Ext.getCmp('STATUS_1').getValue() == '經辦退回' && Ext.getCmp('LOGIN_USERID').getValue() == Ext.getCmp('EDITOR_1').getValue())
                        Ext.getCmp('btnCancel').setVisible(true);
                    else if (Ext.getCmp('STATUS_1').getValue() == '主管退回' && Ext.getCmp('LOGIN_USERID').getValue() == Ext.getCmp('EDITOR_1').getValue())
                        Ext.getCmp('btnCancel').setVisible(true);
                    else
                        Ext.getCmp('btnCancel').setVisible(false);

                    var grid = Ext.ComponentQuery.query('grid')[0];
                    var col = grid.getView().getHeaderCt().getHeaderAtIndex(1);
                    col.show();
                }
            }, '-',
            {
                text: '聯絡人清單',
                cls: 'funBtn',
                handler: function () {
                    popContact();
                }
            }, '-',
            {
                id: 'fileMgrBtn',
                itemId: 'fileMgrBtn',
                cls: 'funBtn',
                text: '上傳管理',
                //hidden: true,
                handler: function () {
                    if (!Ext.getCmp('viewMode').disabled && Ext.getCmp('editMode').disabled)
                        showUploadWindow(true, true, 'SP01_DETAIL:' + mlifnr, '上傳檔案[供應商:' + mlifnr + ']', viewport, { closeCallback: reloadFileList });
                    else
                        showUploadWindow(false, false, 'SP01_DETAIL:' + mlifnr, '上傳檔案[供應商:' + mlifnr + ']', viewport, { closeCallback: reloadFileList });
                }
            }, '-',
            //{
            //    text: '可報價物料清單',
            //    cls: 'funBtn',
            //    handler: function () {
            //        popList();
            //    }
            //}, '-',
            {
                text: '證照管理',
                cls: 'funBtn',
                handler: function () {
                    popLicense(mlifnr);
                }
            }, '-',
            {
                text: '簽核歷程',
                cls: 'funBtn',
                handler: function () {
                    popRecord();
                }
            }, '-',
        ]
    });

    //var popContextMenu = function (xy, record, store) {
    //    contextMenu = new Ext.menu.Menu({
    //        items: [
    //            {
    //                text: '刪除',
    //                //iconCls: 'icon-accept',
    //                handler: function () {
    //                    Ext.Msg.confirm('刪除', '確定要刪除此筆資料嗎?', function (button) {
    //                        if (button === 'yes') {
    //                            Ext.Ajax.request({
    //                                url: '../../../api/SP/DeleteKtokk',
    //                                method: reqVal_p,
    //                                params: {
    //                                    vid1: record.get('VID1')
    //                                },
    //                                //async: true,
    //                                success: function (response) {
    //                                    var data = Ext.decode(response.responseText);
    //                                    if (data.success) {
    //                                    }
    //                                },
    //                                failure: function (response) {
    //                                    Ext.MessageBox.alert('錯誤', '發生例外錯誤');
    //                                }
    //                            });

    //                            store.removeAt(store.find('VID1', record.get('VID1')));
    //                        }
    //                    });
    //                }
    //            }
    //        ]
    //    });

    //    contextMenu.showAt(xy);
    //}

    var popContextMenu_contact = function (xy, record, store) {
        contextMenu = new Ext.menu.Menu({
            items: [
                {
                    text: '刪除',
                    //iconCls: 'icon-accept',
                    handler: function () {
                        Ext.Msg.confirm('刪除', '確定要刪除此筆資料嗎?', function (button) {
                            if (button === 'yes') {
                                Ext.Ajax.request({
                                    url: '../../../api/SP/DeleteContact',
                                    method: reqVal_p,
                                    params: {
                                        vid5: record.get('VID5')
                                    },
                                    //async: true,
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                        }
                                    },
                                    failure: function (response) {
                                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                    }
                                });

                                store.removeAt(store.find('VID5', record.get('VID5')));
                            }
                        });
                    }
                }
            ]
        });

        contextMenu.showAt(xy);
    }

    var rowEditing = Ext.create('Ext.grid.plugin.RowEditing', {
        //clicksToMoveEditor: 1,
        clicksToEdit: 1,
        autoCancel: false,
        saveBtnText: '更新',
        cancelBtnText: '取消',
        listeners: {
            beforeedit: function (editor, e, eOpts) {
                if (Ext.getCmp('RLNO').getValue() == 'PGRM') {
                    return false;  // 取消row editing模式
                }
                else {
                    var grid = Ext.ComponentQuery.query('grid')[0];
                    var col = grid.getView().getHeaderCt().getHeaderAtIndex(0);
                    if (e.record.data.LIFNR != '' && e.record.data.LIFNR != null)
                        col.setEditor(null);
                    else {
                        col.setEditor({
                            xtype: 'combo',
                            displayField: 'TEXT1',
                            valueField: 'KTOKK',
                            store: ktokkStore
                        });
                    }
                    //var cols = grid.getView().getHeaderCt().getGridColumns();
                    //Ext.each(cols, function (col) {
                    //    if (col.text == "原料記號") {
                    //        col.setEditor(null);
                    //    }
                    //});

                    //if (context.column.dataIndex == 'PARAU')
                    //col.setEditor(null);
                }
            },
            edit: function (editor, context, eOpts) {
                var data = { item: [] };
                data.item.push(context.record.data);

                //alert(context.record.data['ZTERM']);
                Ext.Ajax.request({
                    url: '../../../api/SP/UpdateKtokk',
                    method: reqVal_p,
                    success: function (response) {
                        var data = Ext.decode(response.responseText);
                        if (data.success) {
                        }
                    },
                    failure: function (response) {
                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                    },
                    jsonData: data
                });
            }
        }
    });

    var cellEditing = Ext.create('Ext.grid.plugin.CellEditing', {
        //clicksToMoveEditor: 1,
        clicksToEdit: 1,
        autoCancel: false,
        //listeners: {
        //    beforeedit: function (editor, e, eOpts) {
        //        if (Ext.getCmp('RLNO').getValue() == 'PGRM') {
        //            return false;  // 取消cell editing模式
        //        }
        //    }
        //}
    });

    var T1Form = Ext.widget({
        xtype: 'form',
        id: 'SP01_detail',
        //bodyStyle: 'margin:5px;border:none',
        //layout: {
        //    type: 'vbox'
        //    //tdAttrs: { valign: 'top' }
        //},
        autoScroll: true,
        //layout: 'auto',
        items: [
            topTool,
            {
                xtype: 'fieldset',
                width: 800,
                title: '基本資料',
                autoHeight: true,
                style: "margin:5px;background-color: #ecf5ff;",
                cls: 'fieldset-title-bigsize',
                layout: 'hbox',
                items: [
                    {

                        xtype: 'displayfield',
                        id: 'KONZS',
                        style: 'margin-bottom:5px;',
                        fieldStyle: 'font-weight:bold;',
                        fieldLabel: '群組供應商編號',
                        labelAlign: 'right',
                    },
                    {
                        xtype: 'displayfield',
                        id: 'STCEG',
                        fieldStyle: 'font-weight:bold;',
                        fieldLabel: '統一編號',
                        labelAlign: 'right'
                    },
                    {
                        xtype: 'displayfield',
                        colspan: 2,
                        fieldStyle: 'font-weight:bold;',
                        fieldLabel: '國別',
                        id: 'COUNTRY',
                        labelAlign: 'right'
                    },

                ]
            },
            {
                xtype: 'fieldset',
                //columnWidth: 0.5,
                title: '資料維護',
                autoHeight: true,
                width: 800,
                //cls: 'my-fieldset',
                style: "margin:5px;background-color: #ecf5ff;",
                cls: 'fieldset-title-bigsize',
                //collapsible: false,
                //defaultType: 'textfield',
                //defaults: { anchor: '70%' },
                layout: 'anchor',
                items: [
                    {
                        xtype: 'container',
                        //width: 800,
                        layout: {
                            type: 'table',
                            columns: 1,
                        },
                        items: [
                            {
                                xtype: 'hiddenfield',
                                id: 'RLNO',
                                name: 'RLNO',
                                value: ''
                            },
                            {
                                xtype: 'hiddenfield',
                                id: 'KONZS1',
                                name: 'KONZS1',
                                value: ''
                            },
                            {
                                xtype: 'hiddenfield',
                                id: 'ACTION',
                                name: 'ACTION',
                                value: ''
                            },
                            {
                                xtype: 'hiddenfield',
                                id: 'LOGIN_USERID',
                                name: 'LOGIN_USERID',
                                value: ''
                            },
                            {
                                xtype: 'hiddenfield',
                                id: 'EDITOR_1',
                                name: 'EDITOR_1',
                                value: ''
                            },
                            {
                                xtype: 'hiddenfield',
                                id: 'STATUS_1',
                                name: 'STATUS_1',
                                value: ''
                            },
                            {
                                //xtype: 'container',
                                //layout: 'fit',
                                //style: { marginBottom: '4px' },
                                //items: [
                                //    {
                                //        xtype: 'textfield',
                                //        id: 'NAME1',
                                //        fieldLabel: '供應商名稱(中)',
                                //        labelAlign: 'right',
                                //        //allowBlank: false
                                //    }
                                //]
                                xtype: 'textfield',
                                id: 'NAME1',
                                name: 'NAME1',
                                fieldLabel: '供應商名稱(中)',
                                labelAlign: 'right',
                                labelWidth: 120,
                                width: 600,
                                //allowBlank: false
                            },
                            {
                                xtype: 'textfield',
                                id: 'NAME2',
                                name: 'NAME2',
                                fieldLabel: '供應商名稱(英)',
                                labelAlign: 'right',
                                labelWidth: 120,
                                width: 600
                            },
                            {
                                xtype: 'textfield',
                                id: 'TEL_NUMBER',
                                name: 'TEL_NUMBER',
                                fieldLabel: '<span style="font-weight:bold;font-size:12px;color:red">*</span>公司電話',
                                //allowBlank: false,
                                labelAlign: 'right',
                                labelWidth: 120,
                                width: 250
                            },
                            {
                                xtype: 'textfield',
                                id: 'FAX_NUMBER',
                                name: 'FAX_NUMBER',
                                fieldLabel: '公司傳真',
                                labelAlign: 'right',
                                labelWidth: 120,
                                width: 250
                            },
                            {
                                xtype: 'textfield',
                                id: 'SMTP_ADDR',
                                name: 'SMTP_ADDR',
                                fieldLabel: '公司Email',
                                labelAlign: 'right',
                                labelWidth: 120,
                                width: 400,
                                listeners: {
                                    blur: function () {
                                        var ereg = /^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$/;
                                        var testResult = ereg.test(T1Form.getForm().findField('COM_EMAIL').getValue());
                                        if (!testResult) {
                                            T1Form.getForm().findField('COM_EMAIL').setValue('');
                                            Ext.MessageBox.alert('錯誤', 'Email格式不符');
                                        }

                                    }
                                }
                            },
                            {
                                xtype: 'textfield',
                                id: 'URL',
                                name: 'URL',
                                fieldLabel: '公司網址',
                                labelAlign: 'right',
                                labelWidth: 120,
                                width: 400
                            },
                            {
                                xtype: 'textfield',
                                id: 'ADDR',
                                name: 'ADDR',
                                fieldLabel: '<span style="font-weight:bold;font-size:12px;color:red">*</span>公司地址',
                                labelAlign: 'right',
                                //allowBlank: false,
                                labelWidth: 120,
                                width: 600
                            },
                            {
                                xtype: 'textfield',
                                id: 'POST_CODE1',
                                name: 'POST_CODE1',
                                fieldLabel: '郵遞區號',
                                labelAlign: 'right',
                                labelWidth: 120,
                                width: 200
                            },
                            {
                                xtype: 'textfield',
                                id: 'J_1KFREPRE',
                                name: 'J_1KFREPRE',
                                fieldLabel: '負責人',
                                labelAlign: 'right',
                                labelWidth: 120,
                                width: 200
                            },
                            {
                                xtype: 'textfield',
                                id: 'ADDRC',
                                name: 'ADDRC',
                                fieldLabel: '通訊地址',
                                labelAlign: 'right',
                                labelWidth: 120,
                                width: 600
                            },
                            {
                                xtype: 'textfield',
                                id: 'ADDRS',
                                name: 'ADDRS',
                                fieldLabel: '倉庫地址',
                                labelAlign: 'right',
                                labelWidth: 120,
                                width: 600
                            },
                            {
                                xtype: 'textfield',
                                id: 'CORPORATION',
                                name: 'CORPORATION',
                                fieldLabel: '供應商母公司',
                                labelAlign: 'right',
                                labelWidth: 120,
                                width: 600
                            },
                            {
                                xtype: 'displayfield',
                                id: 'ATTACHMENT',
                                name: 'ATTACHMENT',
                                fieldLabel: '附件',
                                labelAlign: 'right',
                                labelWidth: 120,
                                width: 600
                            },
                        ]
                    }
                ]
            },
            //{
            //    xtype: 'fieldset',
            //    //columnWidth: 0.5,
            //    title: '附件上傳',
            //    autoHeight: true,
            //    width: 800,
            //    //cls: 'my-fieldset',
            //    style: "margin:5px;background-color: #ecf5ff;",
            //    cls: 'fieldset-title-bigsize',
            //    //collapsible: false,
            //    //defaultType: 'textfield',
            //    //defaults: { anchor: '70%' },
            //    layout: 'anchor',
            //    items: [
            //        {
            //            xtype: 'container',
            //            //width: 800,
            //            layout: {
            //                type: 'table',
            //                columns: 2
            //            },
            //            items: [
            //                {
            //                    xtype: 'label',
            //                    text: '公司合法登記或證明文件:',
            //                    padding: '0 0 0 35',
            //                    labelAlign: 'right',
            //                },
            //                {
            //                    xtype: 'label',
            //                    html: '<a href="javascript:" >xxx.pdf bbb.pdf ccc.pdf</a>',
            //                    padding: '0 0 0 0',
            //                    //colspan: 4,
            //                },
            //                {
            //                    xtype: 'label',
            //                    text: '其他附件:',
            //                    padding: '0 0 0 35',
            //                    labelAlign: 'right',
            //                },
            //                {
            //                    xtype: 'label',
            //                    html: '<a href="javascript:" >xxx.pdf bbb.pdf ccc.pdf</a>',
            //                    padding: '0 0 0 0',
            //                    //colspan: 4,
            //                }
            //            ]
            //        }
            //    ]
            //},
            {
                xtype: 'fieldset',
                id: 'advInfo',
                //columnWidth: 0.5,
                title: '採購經辦維護資料',
                autoHeight: true,
                width: 800,
                style: "margin:5px;background-color: #ecf5ff;",
                cls: 'fieldset-title-bigsize',
                //collapsible: false,
                //defaultType: 'textfield',
                //defaults: { anchor: '70%' },
                layout: 'anchor',
                items: [
                    {
                        xtype: 'button',
                        text: '新增',
                        tooltip: '新增科目群組',
                        iconCls: 'BtnAdd',
                        id: 'btnAddKtokk',
                        //iconCls: 'MGRPClear',
                        handler: function () {
                            var konzs = Ext.getCmp('KONZS').getValue();
                            var vid1;
                            //Ext.Ajax.request({
                            //    url: '../../../api/SP/AddKtokk',
                            //    method: reqVal_p,
                            //    params: {
                            //        KONZS: konzs
                            //    },
                            //    async: false,
                            //    success: function (response) {
                            //        Ext.Ajax.request({
                            //            url: '../../../api/SP/QueryCurrentKtokk',
                            //            method: reqVal_p,
                            //            params: {
                            //                KONZS: konzs
                            //            },
                            //            async: false,
                            //            success: function (response) {
                            //                var data = Ext.decode(response.responseText);
                            //                if (data.success) {
                            //                    vid1 = data.etts[0].VID1;
                            //                }

                            //                var r = Ext.ModelManager.create({
                            //                    VID1: vid1, KONZS: konzs
                            //                }, 'T3Model');

                            //                T3Store.add(r.copy());
                            //            },
                            //            failure: function (response) {
                            //                Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                            //            }
                            //        });
                            //    },
                            //    failure: function (response) {
                            //        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                            //    }
                            //});

                            var r = Ext.ModelManager.create({
                                KONZS: konzs
                            }, 'T3Model');
                            T3Store.add(r.copy());

                            if (T3Store.getCount() == 3)
                                Ext.getCmp('btnAddKtokk').setVisible(false);
                            else
                                Ext.getCmp('btnAddKtokk').setVisible(true);
                        }
                    },
                    //{
                    //    xtype: 'button',
                    //    text: '刪除',
                    //    tooltip: '刪除科目群組',
                    //    iconCls: 'BtnDel',
                    //    id: 'btnDelKtokk',
                    //    handler: function () {
                    //        var selection = this.getView().getSelectionModel().getSelection()

                    //        //store.removeAt(store.find('VID1', record.get('VID1')));
                    //    }
                    //},
                    {
                        xtype: 'grid',
                        itemId: 'grid_ktokk',
                        id: 'grid_ktokk',
                        store: T3Store,
                        height: 140,
                        sortableColumns: false,
                        selModel: {
                            selType: 'cellmodel'
                        },
                        //plugins: [rowEditing],
                        plugins: [cellEditing],
                        //listeners: {
                        //    'itemcontextmenu': function (grid, record, item, index, e, eOpts) {
                        //        e.preventDefault(); //先取消瀏覽器上的右鍵事件處理
                        //        grid.getSelectionModel().select(record);
                        //        //contextmenu.showAt(e.getXY());

                        //        // 已經有vender id的則不顯示刪除
                        //        if (record.data['LIFNR'] == null || record.data['LIFNR'] == '')
                        //            popContextMenu(e.getXY(), record, T3Store);
                        //    }
                        //},
                        columns: [
                            {
                                xtype: 'rownumberer'
                            },
                            {
                                xtype: 'actioncolumn',
                                width: 30,
                                items: [{
                                    icon: '../../../Images/TRA/delete.gif',
                                    tooltip: '刪除此科目群組',
                                    handler: function (grid, rowIndex, colIndex) {
                                        if (Ext.getCmp('viewMode').disabled) {
                                            Ext.MessageBox.alert('訊息', '瀏覽模式下無法刪除');
                                            return;
                                        }
                                        // 已經有vender id的則不顯示刪除
                                        if (T3Store.getAt(rowIndex).data['LIFNR'] != null && T3Store.getAt(rowIndex).data['LIFNR'] != '') {
                                            Ext.MessageBox.alert('訊息', '已有供應商代碼，故不能刪除');
                                            return;
                                        }
                                        Ext.Msg.confirm('刪除', '確定要刪除此筆資料嗎?', function (button) {
                                            if (button === 'yes') {
                                                Ext.Ajax.request({
                                                    url: '../../../api/SP/DeleteKtokk',
                                                    method: reqVal_p,
                                                    params: {
                                                        vid1: T3Store.getAt(rowIndex).data['VID1']
                                                    },
                                                    //async: true,
                                                    success: function (response) {
                                                        var data = Ext.decode(response.responseText);
                                                        if (data.success) {
                                                        }
                                                    },
                                                    failure: function (response) {
                                                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                                    }
                                                });
                                                T3Store.removeAt(rowIndex);

                                                if (T3Store.getCount() == 3)
                                                    Ext.getCmp('btnAddKtokk').setVisible(false);
                                                else
                                                    Ext.getCmp('btnAddKtokk').setVisible(true);
                                            }
                                        });

                                    }
                                }]
                            },
                            {
                                text: '<span style="color:red">*</span>科目群組',
                                dataIndex: 'KTOKK',
                                align: 'left',
                                style: 'text-align:center',
                                width: 80,
                                //locked: true,
                                //editor: {
                                //    xtype: 'combo',
                                //    displayField: 'TEXT1',
                                //    valueField: 'KTOKK',
                                //    store: ktokkStore
                                //},
                                renderer: function (value, metaData, record) {
                                    if (record.data['KTOKK'] == '1ROH')
                                        return '1原料';
                                    else if (record.data['KTOKK'] == '2MAT')
                                        return '2包材';
                                    else if (record.data['KTOKK'] == '3NOR')
                                        return '3一般物品';
                                },
                                getEditor: function (record) {
                                    if (record.data['LIFNR'] == null || record.data['LIFNR'] == '') {
                                        if (Ext.getCmp('editMode').disabled) {

                                            return Ext.create('Ext.form.field.ComboBox', {
                                                id: 'ktokkCombo',
                                                //forceSelection: true,
                                                //store: [[1, 'Option 1'], [2, 'Option 2']]
                                                displayField: 'TEXT1',
                                                valueField: 'KTOKK',
                                                //emptyText: '--請選擇--',
                                                editable: false,
                                                queryMode: 'local',
                                                //store: ktokkStore,
                                                listeners: {
                                                    focus: function () {
                                                        //Ext.getCmp('ktokkCombo').bindStore('ktokk3Store');
                                                        var ktokk1 = false, ktokk2 = false, ktokk3 = false;
                                                        var result = 0;
                                                        var records = T3Store.getRange();
                                                        for (var a = 0; a < records.length; a++) {
                                                            if (records[a].data['KTOKK'] == '1ROH')
                                                                result += 1;
                                                            else if (records[a].data['KTOKK'] == '2MAT')
                                                                result += 2;
                                                            else if (records[a].data['KTOKK'] == '3NOR')
                                                                result += 4;
                                                        }

                                                        if (result == 0)
                                                            Ext.getCmp('ktokkCombo').bindStore('ktokk1Store');
                                                        else if (result == 1)
                                                            Ext.getCmp('ktokkCombo').bindStore('ktokk4Store');
                                                        else if (result == 2)
                                                            Ext.getCmp('ktokkCombo').bindStore('ktokk3Store');
                                                        else if (result == 3)
                                                            Ext.getCmp('ktokkCombo').bindStore('ktokk7Store');
                                                        else if (result == 4)
                                                            Ext.getCmp('ktokkCombo').bindStore('ktokk2Store');
                                                        else if (result == 5)
                                                            Ext.getCmp('ktokkCombo').bindStore('ktokk6Store');
                                                        else if (result == 6)
                                                            Ext.getCmp('ktokkCombo').bindStore('ktokk5Store');
                                                        else if (result == 7)
                                                            Ext.getCmp('ktokkCombo').bindStore('ktokk8Store');
                                                    },
                                                    change: function (combo, newValue, oldValue) {
                                                        //清空聯絡人標記
                                                        var records = T1Store.getRange();
                                                        for (var i = 0; i < records.length; i++) {
                                                            if (oldValue == '1ROH')
                                                                records[i].set('RMARK', '');
                                                            else if (oldValue == '2MAT')
                                                                records[i].set('PMARK', '');
                                                            else if (oldValue == '3NOR')
                                                                records[i].set('GMARK', '');
                                                        }
                                                    }
                                                }
                                            })

                                        }
                                    }
                                    else return false;
                                }
                            },
                            {
                                text: "供應商代碼",
                                dataIndex: 'LIFNR',
                                align: 'left',
                                style: 'text-align:center',
                                width: 80,
                            },
                            {
                                text: '<span style="color:red">*</span>付款條件',
                                dataIndex: 'ZTERM',
                                align: 'left',
                                style: 'text-align:center',
                                width: 400,
                                //locked: true,
                                //editor: {
                                //    xtype: 'combo',
                                //    displayField: 'TEXT1',
                                //    valueField: 'ZTERM',
                                //    store: T7Store
                                //},
                                getEditor: function (record) {
                                    if (!Ext.getCmp('viewMode').disabled && Ext.getCmp('editMode').disabled) {
                                        return Ext.create('Ext.form.field.ComboBox', {
                                            displayField: 'TEXT1',
                                            valueField: 'ZTERM',
                                            //emptyText: '--請選擇--',
                                            store: T7Store,
                                            editable: true,
                                            typeAhead: true,
                                            forceSelection: true,
                                            selectOnFocus: true,
                                            queryMode: 'local',
                                            triggerAction: 'all',
                                            listeners: {
                                                // anyMatch
                                                beforequery: function (record) {
                                                    record.query = new RegExp(record.query, 'i');
                                                    record.forceAll = true;
                                                },
                                                change: {
                                                    fn: function (field, newValue, oldValue, options) {
                                                        //alert(newValue);
                                                        //var v = this.getValue();
                                                        //var rec = this.findRecord(this.valueField || this.displayField, v);
                                                        //var index = this.store.indexOf(rec);
                                                        if (newValue != null) {
                                                            var selectedRecord = Ext.getCmp('grid_ktokk').getSelectionModel().getSelection()[0];
                                                            var rowIndex = Ext.getCmp('grid_ktokk').store.indexOf(selectedRecord);

                                                            var records = T3Store.getRange();
                                                            //if (this.store.getAt(index).data['ZTERM'] != '' && this.store.getAt(index).data['ZTERM'] != null) {
                                                            for (i = 0; i < T5Store.getCount(); i++) {
                                                                //if (this.store.getAt(index).data['ZTERM'].substring(0, 1) == T5Store.getAt(i).data['ZLSCH'])
                                                                //    record.set('ZWELS', this.store.getAt(index).data['ZTERM'].substring(0, 1));
                                                                if (newValue.substring(0, 1) == T5Store.getAt(i).data['ZLSCH'])
                                                                    records[rowIndex].set('ZWELS', newValue.substring(0, 1));
                                                            }

                                                        //}
                                                        }

                                                    }
                                                }
                                            }
                                        })
                                    }
                                    else return false;
                                },
                                renderer: function (value, metaData, record) {
                                    var rtn;
                                    //var zterm = record.data['ZTERM'].split('-');
                                    Ext.Ajax.request({
                                        url: '../../../api/NM/GetZterm',
                                        method: reqVal_p,
                                        params: {
                                            p0: record.data['ZTERM']
                                            //p0: zterm[0],
                                            //p1: zterm[1]
                                        },
                                        async: false,
                                        success: function (response) {
                                            var data = Ext.decode(response.responseText);
                                            if (data.success) {
                                                rtn = data.etts[0].TEXT1;
                                                //alert(data.etts[0].TEXT1);
                                            }
                                        },
                                        failure: function (response) {
                                            Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                        }
                                    });
                                    return rtn;
                                }
                            },
                            {
                                text: '<span style="color:red">*</span>付款方式',
                                dataIndex: 'ZWELS',
                                align: 'left',
                                style: 'text-align:center',
                                width: 170,
                                //editor: {
                                //    xtype: 'combo',
                                //    displayField: 'TEXT1',
                                //    valueField: 'ZLSCH',
                                //    store: T5Store
                                //},
                                getEditor: function (record) {
                                    if (!Ext.getCmp('viewMode').disabled && Ext.getCmp('editMode').disabled) {

                                        return Ext.create('Ext.form.field.ComboBox', {
                                            displayField: 'TEXT1',
                                            valueField: 'ZLSCH',
                                            store: T5Store,
                                            editable: true,
                                            typeAhead: true,
                                            forceSelection: true,
                                            selectOnFocus: true,
                                            queryMode: 'local',
                                            triggerAction: 'all',
                                            listeners: {
                                                // anyMatch
                                                beforequery: function (record) {
                                                    record.query = new RegExp(record.query, 'i');
                                                    record.forceAll = true;
                                                },
                                            }
                                        })

                                    }
                                    else return false;
                                },
                                renderer: function (value, metaData, record) {
                                    var rtn;
                                    Ext.Ajax.request({
                                        url: '../../../api/NM/GetZwels',
                                        method: reqVal_p,
                                        params: {
                                            p0: record.data['ZWELS']
                                        },
                                        async: false,
                                        success: function (response) {
                                            var data = Ext.decode(response.responseText);
                                            if (data.success) {
                                                rtn = data.etts[0].TEXT1;
                                            }
                                        },
                                        failure: function (response) {
                                            Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                        }
                                    });
                                    return rtn;
                                }
                            },
                            {
                                text: '<span style="color:red">*</span>訂單貨幣',
                                dataIndex: 'WAERS',
                                align: 'left',
                                style: 'text-align:center',
                                width: 80,
                                //editor: {
                                //    xtype: 'combo',
                                //    displayField: 'WAERS',
                                //    valueField: 'WAERS',
                                //    store: T11Store
                                //},
                                getEditor: function (record) {
                                    if (!Ext.getCmp('viewMode').disabled && Ext.getCmp('editMode').disabled) {
                                        return Ext.create('Ext.form.field.ComboBox', {
                                            displayField: 'WAERS',
                                            valueField: 'WAERS',
                                            store: T11Store,
                                            editable: true,
                                            typeAhead: true,
                                            forceSelection: true,
                                            selectOnFocus: true,
                                            queryMode: 'local',
                                            triggerAction: 'all',
                                            listeners: {
                                                // anyMatch
                                                beforequery: function (record) {
                                                    record.query = new RegExp(record.query, 'i');
                                                    record.forceAll = true;
                                                },
                                                change: {
                                                    fn: function (field, newValue, oldValue, options) {
                                                        var selectedRecord = Ext.getCmp('grid_ktokk').getSelectionModel().getSelection()[0];
                                                        var rowIndex = Ext.getCmp('grid_ktokk').store.indexOf(selectedRecord);
                                                        var records = T3Store.getRange();

                                                        if (newValue == 'TWD')
                                                            records[rowIndex].set('INCO1', '');
                                                    }
                                                },
                                            }
                                        })
                                    }
                                    else return false;
                                },
                            },
                            {
                                text: '<span style="color: red">*</span>國貿條件1',
                                dataIndex: 'INCO1',
                                align: 'left',
                                style: 'text-align:center',
                                width: 180,
                                //editor: {
                                //    xtype: 'combo',
                                //    displayField: 'TEXT1',
                                //    valueField: 'INCO1',
                                //    store: T6Store
                                //},
                                getEditor: function (record) {
                                    var selectedRecord = Ext.getCmp('grid_ktokk').getSelectionModel().getSelection()[0];
                                    var rowIndex = Ext.getCmp('grid_ktokk').store.indexOf(selectedRecord);
                                    var records = T3Store.getRange();

                                    if (!Ext.getCmp('viewMode').disabled && Ext.getCmp('editMode').disabled && records[rowIndex].data['WAERS'] != 'TWD') {
                                        return Ext.create('Ext.form.field.ComboBox', {
                                            displayField: 'TEXT1',
                                            valueField: 'INCO1',
                                            store: T6Store,
                                            //emptyText: '--請選擇--',
                                            editable: true,
                                            typeAhead: true,
                                            forceSelection: true,
                                            selectOnFocus: true,
                                            queryMode: 'local',
                                            triggerAction: 'all',
                                            listeners: {
                                                // anyMatch
                                                beforequery: function (record) {
                                                    record.query = new RegExp(record.query, 'i');
                                                    record.forceAll = true;
                                                },
                                            }
                                        })
                                    }
                                    else return false;
                                },
                                renderer: function (value, metaData, record) {
                                    var rtn;
                                    Ext.Ajax.request({
                                        url: '../../../api/NM/GetInco1',
                                        method: reqVal_p,
                                        params: {
                                            p0: record.data['INCO1']
                                        },
                                        async: false,
                                        success: function (response) {
                                            var data = Ext.decode(response.responseText);
                                            if (data.success) {
                                                rtn = data.etts[0].TEXT1;
                                            }
                                        },
                                        failure: function (response) {
                                            Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                        }
                                    });
                                    return rtn;
                                }
                            },
                            {
                                text: '<span style="color: red">*</span>國貿條件2',
                                dataIndex: 'INCO2',
                                align: 'left',
                                style: 'text-align:center',
                                width: 200,
                                getEditor: function (record) {
                                    if (!Ext.getCmp('viewMode').disabled && Ext.getCmp('editMode').disabled) {
                                        return Ext.create('Ext.grid.CellEditor', {
                                            field: 'textfield'
                                        });
                                    }
                                    else return false;
                                }
                            },
                            //{
                            //    text: "狀態",
                            //    dataIndex: 'STATUS',
                            //    align: 'left',
                            //    style: 'text-align:center',
                            //    sortable: false,
                            //    width: 70,
                            //},
                            {
                                text: "凍結",
                                dataIndex: 'MARK1',
                                align: 'left',
                                style: 'text-align:center',
                                width: 70,
                                //editor: {
                                //    xtype: 'combo',
                                //    displayField: 'name',
                                //    valueField: 'value',
                                //    store: ynStore
                                //}
                                getEditor: function (record) {
                                    if (!Ext.getCmp('viewMode').disabled && Ext.getCmp('editMode').disabled) {
                                        return Ext.create('Ext.grid.CellEditor', {
                                            field: Ext.create('Ext.form.field.ComboBox', {
                                                displayField: 'name',
                                                valueField: 'value',
                                                store: xnStore,
                                            })
                                        });
                                    }
                                    else return false;
                                }
                            },
                            {
                                text: "刪除",
                                dataIndex: 'MARK2',
                                align: 'left',
                                style: 'text-align:center',
                                width: 70,
                                //editor: {
                                //    xtype: 'combo',
                                //    displayField: 'name',
                                //    valueField: 'value',
                                //    store: ynStore
                                //}
                                getEditor: function (record) {
                                    if (!Ext.getCmp('viewMode').disabled && Ext.getCmp('editMode').disabled) {
                                        return Ext.create('Ext.grid.CellEditor', {
                                            field: Ext.create('Ext.form.field.ComboBox', {
                                                displayField: 'name',
                                                valueField: 'value',
                                                store: xnStore,
                                            })
                                        });
                                    }
                                    else return false;
                                },
                            },
                        ]
                    }
                ]
            },
            {
                xtype: 'panel',
                id: 'PanelP4',
                border: false,
                style: "margin:5px;",
                layout: 'hbox',
                items: [
                    //{
                    //    xtype: 'button',
                    //    id: 'btnSave',
                    //    text: '暫存',
                    //    //iconCls: 'MGRPSearch',
                    //    handler: function () {
                    //        Ext.getCmp('ACTION').setValue('SAVE');
                    //        save();
                    //    }
                    //},
                    {
                        xtype: 'button',
                        id: 'btnSend',
                        text: '送出',
                        //iconCls: 'MGRPSearch',
                        handler: function () {
                            var ktokk1 = false, ktokk2 = false, ktokk3 = false;
                            var flag1 = false, flag2 = false, flag3 = false;

                            if (Ext.getCmp('RLNO').getValue() == 'PGR' || Ext.getCmp('RLNO').getValue() == 'PGRD' || Ext.getCmp('RLNO').getValue() == 'PGRM') {
                                if (T3Store.getCount() == 0) {
                                    Ext.MessageBox.alert('錯誤', '科目群組不得為空，請新增科目群組');
                                    return;
                                }
                                var records = T3Store.getRange();
                                for (var i = 0; i < records.length; i++) {
                                    if (records[i].data['KTOKK'] == '') {
                                        Ext.MessageBox.alert('錯誤', '項次' + (i + 1) + '，請填寫科目群組');
                                        return;
                                    }
                                    if (records[i].data['ZTERM'] == '') {
                                        Ext.MessageBox.alert('錯誤', '項次' + (i + 1) + '，請填寫付款條件');
                                        return;
                                    }
                                    if (records[i].data['ZWELS'] == '') {
                                        Ext.MessageBox.alert('錯誤', '項次' + (i + 1) + '，請填寫付款方式');
                                        return;
                                    }
                                    if (records[i].data['WAERS'] == '') {
                                        Ext.MessageBox.alert('錯誤', '項次' + (i + 1) + '，請填寫訂單貨幣');
                                        return;
                                    }
                                    if (records[i].data['WAERS'] != 'TWD' && records[i].data['INCO1'] == '') {
                                        Ext.MessageBox.alert('錯誤', '項次' + (i + 1) + '，請填寫國貿條件1');
                                        return;
                                    }
                                    if (records[i].data['WAERS'] != 'TWD' && records[i].data['INCO2'] == '') {
                                        Ext.MessageBox.alert('錯誤', '項次' + (i + 1) + '，請填寫國貿條件2');
                                        return;
                                    }

                                    if (records[i].data['KTOKK'] == '1ROH')
                                        ktokk1 = true;
                                    else if (records[i].data['KTOKK'] == '2MAT')
                                        ktokk2 = true;
                                    else if (records[i].data['KTOKK'] == '3NOR')
                                        ktokk3 = true;
                                }
                            }

                            records = T1Store.getRange();
                            for (var i = 0; i < records.length; i++) {
                                if (records[i].data['NAME_LAST'] == '') {
                                    Ext.MessageBox.alert('訊息', '聯絡人項次' + (i + 1) + '，請填寫姓');
                                    return;
                                }
                                if (records[i].data['NAME_FIRST'] == '') {
                                    Ext.MessageBox.alert('訊息', '聯絡人項次' + (i + 1) + '，請填寫名');
                                    return;
                                }
                                if (records[i].data['ABTNR'] == '') {
                                    Ext.MessageBox.alert('訊息', '聯絡人項次' + (i + 1) + '，請填寫部門');
                                    return;
                                }
                                if (records[i].data['PAFKT'] == '') {
                                    Ext.MessageBox.alert('訊息', '聯絡人項次' + (i + 1) + '，請填寫職稱');
                                    return;
                                }
                                if (records[i].data['TEL_NUMBER'] == '') {
                                    Ext.MessageBox.alert('訊息', '聯絡人項次' + (i + 1) + '，請填寫電話1');
                                    return;
                                }
                                if (records[i].data['SMTP_ADDR'] == '') {
                                    Ext.MessageBox.alert('訊息', '聯絡人項次' + (i + 1) + '，請填寫EMAIL1');
                                    return;
                                }
                                if (Ext.getCmp('RLNO').getValue() == 'PGR' || Ext.getCmp('RLNO').getValue() == 'PGRD' || Ext.getCmp('RLNO').getValue() == 'PGRM') {
                                    if (records[i].data['RMARK'] == '' && records[i].data['PMARK'] == '' && records[i].data['GMARK'] == '') {
                                        Ext.MessageBox.alert('訊息', '聯絡人的三種科目群組記號至少要有一組選擇Y');
                                        return;
                                    }
                                    if (records[i].data['PAVIP'] == '1' && records[i].data['RMARK'] == '' && records[i].data['PMARK'] == '' && records[i].data['GMARK'] == '') {
                                        Ext.MessageBox.alert('訊息', '已選擇為主要聯絡人，則三種科目群組記號至少要有一組選擇Y');
                                        return;
                                    }
                                }
                            }

                            if (ktokk1) {
                                for (var i = 0; i < records.length; i++) {
                                    if (records[i].data['RMARK'] == 'Y')
                                        flag1 = true;
                                }
                            }
                            if (ktokk2) {
                                for (var i = 0; i < records.length; i++) {
                                    if (records[i].data['PMARK'] == 'Y')
                                        flag2 = true;
                                }
                            }
                            if (ktokk3) {
                                for (var i = 0; i < records.length; i++) {
                                    if (records[i].data['GMARK'] == 'Y')
                                        flag3 = true;
                                }
                            }

                            if (Ext.getCmp('RLNO').getValue() == 'PGR' || Ext.getCmp('RLNO').getValue() == 'PGRD' || Ext.getCmp('RLNO').getValue() == 'PGRM') {
                                if (ktokk1 ^ flag1) {
                                    Ext.MessageBox.alert('錯誤', "請於[聯絡人清單]'中, 將一位聯絡人之原料記號設定為\"Y\"");
                                    return;
                                }

                                if (ktokk2 ^ flag2) {
                                    Ext.MessageBox.alert('錯誤', "請於[聯絡人清單]'中, 將一位聯絡人之包材記號設定為\"Y\"");
                                    return;
                                }

                                if (ktokk3 ^ flag3) {
                                    Ext.MessageBox.alert('錯誤', "請於[聯絡人清單]'中, 將一位聯絡人之一般物品記號設定為\"Y\"");
                                    return;
                                }
                            }

                            if (Ext.getCmp('COUNTRY').getValue().trim() == 'TW') {
                                if (Ext.getCmp('NAME1').getValue() == '') {
                                    Ext.Msg.alert('訊息', '請填寫供應商名稱(中)');
                                    return false;
                                }
                                if (Ext.getCmp('POST_CODE1').getValue().trim() != '' && Ext.getCmp('POST_CODE1').getValue().length != 3) {
                                    Ext.Msg.alert('訊息', '郵遞區號必須為3碼');
                                    return false;
                                }
                            }
                            else {
                                if (Ext.getCmp('NAME2').getValue().trim() == '') {
                                    Ext.Msg.alert('訊息', '請填寫供應商名稱(英)');
                                    return false;
                                }
                            }

                            if (Ext.getCmp('TEL_NUMBER').getValue().trim() == '') {
                                Ext.Msg.alert('訊息', '請填寫公司電話');
                                return false;
                            }
                            if (Ext.getCmp('ADDR').getValue().trim() == '') {
                                Ext.Msg.alert('訊息', '請填寫公司地址');
                                return false;
                            }

                            Ext.Msg.confirm('訊息', '確定要送出審核嗎?', function (button) {
                                if (button === 'yes') {
                                    // 所以送主管審核，要自動儲存聯絡人
                                    var t1records = T1Store.getRange();
                                    var data = { item: [] };
                                    for (var i = 0; i < t1records.length; i++) {
                                        data.item.push(t1records[i].data);
                                    }
                                    Ext.Ajax.request({
                                        url: '../../../api/SP/SaveContact',
                                        method: reqVal_p,
                                        success: function (response) {
                                            var data = Ext.decode(response.responseText);
                                            if (data.success) {
                                                T1Store.load();
                                                //Ext.Msg.alert('訊息', '儲存成功');

                                                Ext.getCmp('ACTION').setValue('SEND');
                                                save();
                                            }
                                        },
                                        failure: function (response) {
                                            Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                        },
                                        jsonData: data
                                    });


                                    //Ext.getCmp('ACTION').setValue('SEND');
                                    //save();
                                    //var tp = window.parent.tabs;
                                    //var activeTab = tp.getActiveTab();
                                    //var activeTabIndex = tp.items.findIndex('id', activeTab.id);
                                    //tp.remove(activeTabIndex);
                                }
                            });
                        }
                    },
                    {
                        xtype: 'button',
                        id: 'btnCancel',
                        text: '撤銷異動',
                        handler: function () {
                            Ext.Msg.confirm('訊息', '確定要撤銷嗎?', function (button) {
                                if (button === 'yes') {
                                    Ext.Ajax.request({
                                        url: '../../../api/SP/UpdateStatus',
                                        method: reqVal_p,
                                        params: {
                                            code: 6,
                                            konzs: Ext.getCmp('KONZS1').getValue(),
                                            comment: ''
                                        },
                                        //async: true,
                                        success: function (response) {
                                            var data = Ext.decode(response.responseText);
                                            if (data.success) {
                                                var tp = window.parent.tabs;
                                                var activeTab = tp.getActiveTab();
                                                var activeTabIndex = tp.items.findIndex('id', activeTab.id);
                                                tp.remove(activeTabIndex);
                                            }
                                        },
                                        failure: function (response) {
                                            Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                        }
                                    });
                                }
                            });
                        }
                    },
                ]
            }
        ]
    });

    var save = function () {
        var f = T1Form.getForm();
        //if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();

            f.submit({
                url: '../../../api/SP/TmpSaveBase',
                //headers: {'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' },
                success: function (form, action) {
                    myMask.hide();
                    if (action.result.msg == '') {
                        var records = T3Store.getRange();
                        for (var i = 0; i < records.length; i++) {
                            //alert(records[i].data['KTOKK']);
                            var data = { item: [] };
                            data.item.push(records[i].data);

                            Ext.Ajax.request({
                                url: '../../../api/SP/TmpSaveKtokk',
                                method: reqVal_p,
                                success: function (response) {
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        
                                        Ext.Msg.alert('訊息', '送出成功', callBackFunc);
                                        function callBackFunc(id) {
                                            var tp = window.parent.tabs;
                                            var activeTab = tp.getActiveTab();
                                            var activeTabIndex = tp.items.findIndex('id', activeTab.id);
                                            tp.remove(activeTabIndex);
                                        }
                                    }
                                },
                                failure: function (response) {
                                    Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                },
                                jsonData: data
                            });
                        }
                    }
                    else
                        Ext.Msg.alert('訊息', action.result.msg); // 修改中，無法暫存

                },
                failure: function (form, action) {
                    myMask.hide();
                    switch (action.failureType) {
                        case Ext.form.action.Action.CLIENT_INVALID:
                            Ext.Msg.alert('失敗', 'Form fields may not be submitted with invalid values');
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

        //}
        //else {
        //    Ext.MessageBox.show({
        //        title: "錯誤",
        //        msg: "請填寫必要欄位",
        //        buttons: Ext.MessageBox.OK,
        //        icon: Ext.MessageBox.WARNING
        //    });
        //}
    }

    var rowEditingContact = Ext.create('Ext.grid.plugin.RowEditing', {
        //clicksToMoveEditor: 1,
        clicksToEdit: 1,
        autoCancel: false,
        saveBtnText: '更新',
        cancelBtnText: '取消',
        listeners: {
            beforeedit: function (editor, context, eOpts) {
                //if (Ext.getCmp('RLNO').getValue() == 'PGRM') {
                if (Ext.getCmp('viewMode').disabled) {
                    return false;  // 取消row editing模式
                } else {
                    //var grid = Ext.ComponentQuery.query('grid#grid_contact');
                    var grid = Ext.ComponentQuery.query('grid')[2];
                    //var col = grid.getView().getHeaderCt().getHeaderAtIndex(4);
                    //var cols = grid.getView().getHeaderCt().getGridColumns();
                    //Ext.each(cols, function (col) {
                    //    if (col.text == "原料記號") {
                    //        col.setEditor(null);
                    //    }
                    //});

                    //if (context.column.dataIndex == 'PARAU')
                    //col.setEditor(null);
                }
            },
            edit: function (editor, context, eOpts) {
                //var grid = Ext.ComponentQuery.query('#grid_contact')[0];
                //var store = grid.getView().getStore();

                var data = { item: [] };
                data.item.push(context.record.data);

                //alert(context.record.data['NAME_LAST']);
                Ext.Ajax.request({
                    url: '../../../api/SP/UpdateContact',
                    method: reqVal_p,
                    //params: {
                    //    VID5: context.record.data['VID5']
                    //},
                    //async: true,
                    success: function (response) {
                        var data = Ext.decode(response.responseText);
                        if (data.success) {
                        }
                    },
                    failure: function (response) {
                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                    },
                    jsonData: data
                });
            },
            canceledit: function (editor, context, eOpts) {
            }
        }
    });

    popContact = function () {
        //alert(T1Form.getForm().findField('TMPKEY').getValue());
        if (!win) {
            var popform = Ext.create('Ext.form.Panel', {
                height: '100%',
                //layout: 'fit',
                layout: 'anchor',
                closable: false,
                items: [
                    {
                        xtype: 'button',
                        id: 'btnAddContact',
                        iconCls: 'BtnAdd',
                        text: '新增聯絡人',
                        //iconCls: 'MGRPClear',
                        handler: function () {
                            var vid5;
                            var konzs = Ext.getCmp('KONZS').getValue();
                            //Ext.Ajax.request({
                            //    url: '../../../api/SP/AddContact',
                            //    method: reqVal_p,
                            //    params: {
                            //        KONZS: konzs
                            //    },
                            //    async: false,
                            //    success: function (response) {
                            //        Ext.Ajax.request({
                            //            url: '../../../api/SP/QueryCurrentContact',
                            //            method: reqVal_p,
                            //            params: {
                            //                KONZS: konzs
                            //            },
                            //            async: false,
                            //            success: function (response) {
                            //                var data = Ext.decode(response.responseText);
                            //                if (data.success) {
                            //                    vid5 = data.etts[0].VID5;  // 設定VID5, 才能用於右鍵刪除
                            //                }
                            //                //Ext.getCmp('hidden_state').setValue('Update');

                            //                var r = Ext.ModelManager.create({
                            //                    VID5: vid5, KONZS: konzs
                            //                }, 'T1Model');

                            //                T1Store.add(r.copy());
                            //            },
                            //            failure: function (response) {
                            //                Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                            //            }
                            //        });
                            //    },
                            //    failure: function (response) {
                            //        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                            //    }
                            //});
                            var r = Ext.ModelManager.create({
                                KONZS: konzs
                            }, 'T1Model');
                            T1Store.add(r.copy());
                        }
                    },
                    {
                        xtype: 'grid',
                        height: 400,
                        //title: '引用製造商清單',
                        store: T1Store,
                        id: 'grid_contact',
                        sortableColumns: false,
                        //viewConfig: {
                        //    listeners: {
                        //        // 讓column寬度可以隨著內容自動縮放
                        //        refresh: function (dataview) {
                        //            Ext.each(dataview.panel.columns, function (column) {
                        //                if (column.autoSizeColumn === true)
                        //                    column.autoSize();
                        //            })
                        //        }
                        //    }
                        //},
                        selModel: {
                            selType: 'cellmodel'
                        },
                        //plugins: [rowEditingContact],
                        plugins: {
                            ptype: 'cellediting',
                            clicksToEdit: 1
                        },
                        //listeners: {
                        //    'itemcontextmenu': function (grid, record, item, index, e, eOpts) {
                        //        e.preventDefault(); //先取消瀏覽器上的右鍵事件處理
                        //        grid.getSelectionModel().select(record);
                        //        //contextmenu.showAt(e.getXY());
                        //        // 已經有user id的則不顯示刪除
                        //        if (record.data['USERID'] == null || record.data['USERID'] == '')
                        //            popContextMenu_contact(e.getXY(), record, T1Store);
                        //    }
                        //},
                        columns: [
                            {
                                xtype: 'rownumberer'
                            },
                            {
                                xtype: 'actioncolumn',
                                width: 30,
                                items: [{
                                    icon: '../../../Images/TRA/delete.gif',
                                    tooltip: '刪除此聯絡人',
                                    handler: function (grid, rowIndex, colIndex) {
                                        // 已經有vender id的則不顯示刪除
                                        if (T1Store.getAt(rowIndex).data['USERID'] != null && T1Store.getAt(rowIndex).data['USERID'] != '') {
                                            Ext.MessageBox.alert('訊息', '已有聯絡人ID，故不能刪除');
                                            return;
                                        }

                                        Ext.Msg.confirm('刪除', '確定要刪除此筆資料嗎?', function (button) {
                                            if (button === 'yes') {
                                                //Ext.Ajax.request({
                                                //    url: '../../../api/SP/DeleteContact',
                                                //    method: reqVal_p,
                                                //    params: {
                                                //        vid5: T1Store.getAt(rowIndex).data['VID5']
                                                //    },
                                                //    //async: true,
                                                //    success: function (response) {
                                                //        var data = Ext.decode(response.responseText);
                                                //        if (data.success) {
                                                //        }
                                                //    },
                                                //    failure: function (response) {
                                                //        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                                //    }
                                                //});
                                                T1Store.removeAt(rowIndex);
                                            }
                                        });


                                    }
                                }]
                            },
                            {
                                text: '<span style="color: red">*</span>姓',
                                dataIndex: 'NAME_LAST',
                                align: 'left',
                                style: 'text-align:center',
                                width: 80,
                                //locked: true,
                                getEditor: function (record) {
                                    if (!Ext.getCmp('viewMode').disabled && Ext.getCmp('editMode').disabled && Ext.getCmp('RLNO').getValue() != 'LVA' && Ext.getCmp('RLNO').getValue() != 'FVA') {
                                        return Ext.create('Ext.grid.CellEditor', {
                                            field: 'textfield'
                                        });
                                    }
                                    else return false;
                                }
                            },
                            {
                                text: '<span style="color:red">*</span>名',
                                dataIndex: 'NAME_FIRST',
                                align: 'left',
                                style: 'text-align:center',
                                width: 80,
                                //locked: true,
                                getEditor: function (record) {
                                    if (!Ext.getCmp('viewMode').disabled && Ext.getCmp('editMode').disabled && Ext.getCmp('RLNO').getValue() != 'LVA' && Ext.getCmp('RLNO').getValue() != 'FVA') {
                                        return Ext.create('Ext.grid.CellEditor', {
                                            field: 'textfield'
                                        });
                                    }
                                    else return false;
                                }
                            },
                            {
                                text: "USERID",
                                dataIndex: 'USERID',
                                align: 'center',
                                style: 'text-align:center',
                                width: 80,
                                //locked: true,
                            },
                            {
                                text: "主要聯絡人",
                                dataIndex: 'PAVIP',
                                align: 'center',
                                style: 'text-align:center',
                                width: 70,
                                //editor: {
                                //    xtype: 'combo',
                                //    displayField: 'name',
                                //    valueField: 'value',
                                //    store: vipStore
                                //}
                                getEditor: function (record) {
                                    if (!Ext.getCmp('viewMode').disabled && Ext.getCmp('editMode').disabled && Ext.getCmp('RLNO').getValue() != 'LVA' && Ext.getCmp('RLNO').getValue() != 'FVA') {
                                        return Ext.create('Ext.grid.CellEditor', {
                                            field: Ext.create('Ext.form.field.ComboBox', {
                                                displayField: 'name',
                                                valueField: 'value',
                                                editable: false,
                                                store: vipStore,
                                            })
                                        });
                                    }
                                    else return false;
                                },
                            },
                            {
                                text: '<span style="color:red">*</span>部門',
                                dataIndex: 'ABTNR',
                                align: 'left',
                                style: 'text-align:center',
                                width: 270,
                                //editor: {
                                //    xtype: 'combo',
                                //    displayField: 'TEXT1',
                                //    valueField: 'ABTNR',
                                //    store: T9Store
                                //},
                                getEditor: function (record) {
                                    if (!Ext.getCmp('viewMode').disabled && Ext.getCmp('editMode').disabled && Ext.getCmp('RLNO').getValue() != 'LVA' && Ext.getCmp('RLNO').getValue() != 'FVA') {
                                        return Ext.create('Ext.form.field.ComboBox', {
                                            displayField: 'TEXT1',
                                            valueField: 'ABTNR',
                                            editable: false,
                                            store: T9Store,
                                            editable: true,
                                            typeAhead: true,
                                            forceSelection: true,
                                            selectOnFocus: true,
                                            queryMode: 'local',
                                            triggerAction: 'all',
                                            listeners: {
                                                // anyMatch
                                                beforequery: function (record) {
                                                    record.query = new RegExp(record.query, 'i');
                                                    record.forceAll = true;
                                                },
                                            }
                                        });
                                    }
                                    else return false;
                                },
                                renderer: function (value, metaData, record) {
                                    var rtn;
                                    Ext.Ajax.request({
                                        url: '../../../api/NM/GetABTNR',
                                        method: reqVal_p,
                                        params: {
                                            p0: record.data['ABTNR']
                                        },
                                        async: false,
                                        success: function (response) {
                                            var data = Ext.decode(response.responseText);
                                            if (data.success) {
                                                rtn = data.etts[0].TEXT1;
                                            }
                                        },
                                        failure: function (response) {
                                            Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                        }
                                    });
                                    return rtn;
                                }
                            },
                            {
                                text: '<span style="color:red">*</span>職稱',
                                dataIndex: 'PAFKT',
                                align: 'left',
                                style: 'text-align:center',
                                width: 100,
                                //editor: {
                                //    xtype: 'combo',
                                //    displayField: 'TEXT1',
                                //    valueField: 'PAFKT',
                                //    store: T10Store
                                //},
                                getEditor: function (record) {
                                    if (!Ext.getCmp('viewMode').disabled && Ext.getCmp('editMode').disabled && Ext.getCmp('RLNO').getValue() != 'LVA' && Ext.getCmp('RLNO').getValue() != 'FVA') {
                                        return Ext.create('Ext.form.field.ComboBox', {
                                            displayField: 'TEXT1',
                                            valueField: 'PAFKT',
                                            store: T10Store,
                                            editable: true,
                                            typeAhead: true,
                                            forceSelection: true,
                                            selectOnFocus: true,
                                            queryMode: 'local',
                                            triggerAction: 'all',
                                            listeners: {
                                                // anyMatch
                                                beforequery: function (record) {
                                                    record.query = new RegExp(record.query, 'i');
                                                    record.forceAll = true;
                                                },
                                            }
                                        });
                                    }
                                    else return false;
                                },
                                renderer: function (value, metaData, record) {
                                    var rtn;
                                    Ext.Ajax.request({
                                        url: '../../../api/NM/GetPAFKT',
                                        method: reqVal_p,
                                        params: {
                                            p0: record.data['PAFKT']
                                        },
                                        async: false,
                                        success: function (response) {
                                            var data = Ext.decode(response.responseText);
                                            if (data.success) {
                                                rtn = data.etts[0].TEXT1;
                                            }
                                        },
                                        failure: function (response) {
                                            Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                        }
                                    });
                                    return rtn;
                                }
                            },
                            {
                                text: "備註",
                                dataIndex: 'PARAU',
                                align: 'left',
                                style: 'text-align:center',
                                //autoSizeColumn: true,
                                width: 150,
                                getEditor: function (record) {
                                    if (!Ext.getCmp('viewMode').disabled && Ext.getCmp('editMode').disabled && Ext.getCmp('RLNO').getValue() != 'LVA' && Ext.getCmp('RLNO').getValue() != 'FVA') {
                                        return Ext.create('Ext.grid.CellEditor', {
                                            field: 'textfield'
                                        });
                                    }
                                    else return false;
                                }
                            },
                            {
                                text: "原料記號",
                                dataIndex: 'RMARK',
                                align: 'left',
                                style: 'text-align:center',
                                width: 70,
                                //editor: {
                                //    xtype: 'combo',
                                //    displayField: 'name',
                                //    valueField: 'value',
                                //    store: ynStore
                                //}
                                getEditor: function (record) {
                                    var grid = Ext.ComponentQuery.query('grid')[0];
                                    var flag = false;
                                    for (i = 0; i < grid.getStore().getCount(); i++) {
                                        if (grid.getStore().getAt(i).data['KTOKK'] == '1ROH')
                                            flag = true;
                                    }
                                    if (flag && !Ext.getCmp('viewMode').disabled && Ext.getCmp('editMode').disabled && Ext.getCmp('RLNO').getValue() != 'LVA' && Ext.getCmp('RLNO').getValue() != 'FVA') {
                                        return Ext.create('Ext.grid.CellEditor', {
                                            field: Ext.create('Ext.form.field.ComboBox', {
                                                displayField: 'name',
                                                valueField: 'value',
                                                editable: false,
                                                store: ynStore,
                                            })
                                        });
                                    }
                                    else return false;
                                },
                            },
                            {
                                text: "包材記號",
                                dataIndex: 'PMARK',
                                align: 'left',
                                style: 'text-align:center',
                                width: 70,
                                //editor: {
                                //    xtype: 'combo',
                                //    displayField: 'name',
                                //    valueField: 'value',
                                //    store: ynStore
                                //}
                                getEditor: function (record) {
                                    var grid = Ext.ComponentQuery.query('grid')[0];
                                    var flag = false;
                                    for (i = 0; i < grid.getStore().getCount(); i++) {
                                        if (grid.getStore().getAt(i).data['KTOKK'] == '2MAT')
                                            flag = true;
                                    }
                                    if (flag && !Ext.getCmp('viewMode').disabled && Ext.getCmp('editMode').disabled && Ext.getCmp('RLNO').getValue() != 'LVA' && Ext.getCmp('RLNO').getValue() != 'FVA') {
                                        return Ext.create('Ext.grid.CellEditor', {
                                            field: Ext.create('Ext.form.field.ComboBox', {
                                                displayField: 'name',
                                                valueField: 'value',
                                                editable: false,
                                                store: ynStore,
                                            })
                                        });
                                    }
                                    else return false;
                                },
                            },
                            {
                                text: "一般物品記號",
                                dataIndex: 'GMARK',
                                align: 'left',
                                style: 'text-align:center',
                                width: 90,
                                //editor: {
                                //    xtype: 'combo',
                                //    displayField: 'name',
                                //    valueField: 'value',
                                //    store: ynStore
                                //}
                                getEditor: function (record) {
                                    var grid = Ext.ComponentQuery.query('grid')[0];
                                    var flag = false;
                                    for (i = 0; i < grid.getStore().getCount(); i++) {
                                        if (grid.getStore().getAt(i).data['KTOKK'] == '3NOR')
                                            flag = true;
                                    }

                                    if (flag && !Ext.getCmp('viewMode').disabled && Ext.getCmp('editMode').disabled && Ext.getCmp('RLNO').getValue() != 'LVA' && Ext.getCmp('RLNO').getValue() != 'FVA') {
                                        return Ext.create('Ext.grid.CellEditor', {
                                            field: Ext.create('Ext.form.field.ComboBox', {
                                                displayField: 'name',
                                                valueField: 'value',
                                                editable: false,
                                                store: ynStore,
                                            })
                                        });
                                    }
                                    else return false;
                                },
                            },
                            {
                                text: "登入系統",
                                dataIndex: 'SYS',
                                align: 'left',
                                style: 'text-align:center',
                                width: 70,
                                //editor: {
                                //    xtype: 'combo',
                                //    displayField: 'name',
                                //    valueField: 'value',
                                //    store: ynStore
                                //}
                                getEditor: function (record) {
                                    if (!Ext.getCmp('viewMode').disabled && Ext.getCmp('editMode').disabled && Ext.getCmp('RLNO').getValue() != 'LVA' && Ext.getCmp('RLNO').getValue() != 'FVA') {
                                        return Ext.create('Ext.grid.CellEditor', {
                                            field: Ext.create('Ext.form.field.ComboBox', {
                                                displayField: 'name',
                                                valueField: 'value',
                                                editable: false,
                                                store: ynStore,
                                            })
                                        });
                                    }
                                    else return false;
                                },
                            },
                            {
                                text: "管理者",
                                dataIndex: 'ADMIN1',
                                align: 'left',
                                style: 'text-align:center',
                                width: 50,
                                //editor: {
                                //    xtype: 'combo',
                                //    displayField: 'name',
                                //    valueField: 'value',
                                //    store: ynStore
                                //}
                                getEditor: function (record) {
                                    if (!Ext.getCmp('viewMode').disabled && Ext.getCmp('editMode').disabled && Ext.getCmp('RLNO').getValue() != 'LVA' && Ext.getCmp('RLNO').getValue() != 'FVA') {
                                        return Ext.create('Ext.grid.CellEditor', {
                                            field: Ext.create('Ext.form.field.ComboBox', {
                                                displayField: 'name',
                                                valueField: 'value',
                                                editable: false,
                                                store: ynStore,
                                            })
                                        });
                                    }
                                    else return false;
                                },
                            },
                            {
                                text: "公開",
                                dataIndex: 'OPEN1',
                                align: 'left',
                                style: 'text-align:center',
                                width: 50,
                                //editor: {
                                //    xtype: 'combo',
                                //    displayField: 'name',
                                //    valueField: 'value',
                                //    store: ynStore
                                //}
                                getEditor: function (record) {
                                    if (!Ext.getCmp('viewMode').disabled && Ext.getCmp('editMode').disabled && Ext.getCmp('RLNO').getValue() != 'LVA' && Ext.getCmp('RLNO').getValue() != 'FVA') {
                                        return Ext.create('Ext.grid.CellEditor', {
                                            field: Ext.create('Ext.form.field.ComboBox', {
                                                displayField: 'name',
                                                valueField: 'value',
                                                editable: false,
                                                store: ynStore,
                                            })
                                        });
                                    }
                                    else return false;
                                },
                            },
                            {
                                text: '<span style="color:red">*</span>電話1',
                                dataIndex: 'TEL_NUMBER',
                                align: 'left',
                                style: 'text-align:center',
                                width: 100,
                                getEditor: function (record) {
                                    if (!Ext.getCmp('viewMode').disabled && Ext.getCmp('editMode').disabled && Ext.getCmp('RLNO').getValue() != 'LVA' && Ext.getCmp('RLNO').getValue() != 'FVA') {
                                        return Ext.create('Ext.grid.CellEditor', {
                                            field: 'textfield'
                                        });
                                    }
                                    else return false;
                                }
                            },
                            {
                                text: "分機1",
                                dataIndex: 'TEL_EXTENS',
                                align: 'left',
                                style: 'text-align:center',
                                width: 60,
                                getEditor: function (record) {
                                    if (!Ext.getCmp('viewMode').disabled && Ext.getCmp('editMode').disabled && Ext.getCmp('RLNO').getValue() != 'LVA' && Ext.getCmp('RLNO').getValue() != 'FVA') {
                                        return Ext.create('Ext.grid.CellEditor', {
                                            field: 'textfield'
                                        });
                                    }
                                    else return false;
                                }
                            },
                            {
                                text: "電話1備註",
                                dataIndex: 'TEL_MEMO',
                                align: 'left',
                                style: 'text-align:center',
                                autoSizeColumn: true,
                                getEditor: function (record) {
                                    if (!Ext.getCmp('viewMode').disabled && Ext.getCmp('editMode').disabled && Ext.getCmp('RLNO').getValue() != 'LVA' && Ext.getCmp('RLNO').getValue() != 'FVA') {
                                        return Ext.create('Ext.grid.CellEditor', {
                                            field: 'textfield'
                                        });
                                    }
                                    else return false;
                                }
                            },
                            {
                                text: "電話2",
                                dataIndex: 'TEL_NUMBER2',
                                align: 'left',
                                style: 'text-align:center',
                                width: 100,
                                getEditor: function (record) {
                                    if (!Ext.getCmp('viewMode').disabled && Ext.getCmp('editMode').disabled && Ext.getCmp('RLNO').getValue() != 'LVA' && Ext.getCmp('RLNO').getValue() != 'FVA') {
                                        return Ext.create('Ext.grid.CellEditor', {
                                            field: 'textfield'
                                        });
                                    }
                                    else return false;
                                }
                            },
                            {
                                text: "分機2",
                                dataIndex: 'TEL_EXTENS2',
                                align: 'left',
                                style: 'text-align:center',
                                width: 100,
                                getEditor: function (record) {
                                    if (!Ext.getCmp('viewMode').disabled && Ext.getCmp('editMode').disabled && Ext.getCmp('RLNO').getValue() != 'LVA' && Ext.getCmp('RLNO').getValue() != 'FVA') {
                                        return Ext.create('Ext.grid.CellEditor', {
                                            field: 'textfield'
                                        });
                                    }
                                    else return false;
                                }
                            },
                            {
                                text: "電話2備註",
                                dataIndex: 'TEL_MEMO2',
                                align: 'left',
                                style: 'text-align:center',
                                autoSizeColumn: true,
                                getEditor: function (record) {
                                    if (!Ext.getCmp('viewMode').disabled && Ext.getCmp('editMode').disabled && Ext.getCmp('RLNO').getValue() != 'LVA' && Ext.getCmp('RLNO').getValue() != 'FVA') {
                                        return Ext.create('Ext.grid.CellEditor', {
                                            field: 'textfield'
                                        });
                                    }
                                    else return false;
                                }
                            },
                            {
                                text: "行動電話1",
                                dataIndex: 'MOB_NUMBER',
                                align: 'left',
                                style: 'text-align:center',
                                width: 100,
                                getEditor: function (record) {
                                    if (!Ext.getCmp('viewMode').disabled && Ext.getCmp('editMode').disabled && Ext.getCmp('RLNO').getValue() != 'LVA' && Ext.getCmp('RLNO').getValue() != 'FVA') {
                                        return Ext.create('Ext.grid.CellEditor', {
                                            field: 'textfield'
                                        });
                                    }
                                    else return false;
                                }
                            },
                            {
                                text: "行動電話1備註",
                                dataIndex: 'MOB_MEMO',
                                align: 'left',
                                style: 'text-align:center',
                                autoSizeColumn: true,
                                getEditor: function (record) {
                                    if (!Ext.getCmp('viewMode').disabled && Ext.getCmp('editMode').disabled && Ext.getCmp('RLNO').getValue() != 'LVA' && Ext.getCmp('RLNO').getValue() != 'FVA') {
                                        return Ext.create('Ext.grid.CellEditor', {
                                            field: 'textfield'
                                        });
                                    }
                                    else return false;
                                }
                            },

                            {
                                text: "行動電話2",
                                dataIndex: 'MOB_NUMBER2',
                                align: 'left',
                                style: 'text-align:center',
                                width: 100,
                                getEditor: function (record) {
                                    if (!Ext.getCmp('viewMode').disabled && Ext.getCmp('editMode').disabled && Ext.getCmp('RLNO').getValue() != 'LVA' && Ext.getCmp('RLNO').getValue() != 'FVA') {
                                        return Ext.create('Ext.grid.CellEditor', {
                                            field: 'textfield'
                                        });
                                    }
                                    else return false;
                                }
                            },
                            {
                                text: "行動電話2備註",
                                dataIndex: 'MOB_MEMO2',
                                align: 'left',
                                style: 'text-align:center',
                                autoSizeColumn: true,
                                getEditor: function (record) {
                                    if (!Ext.getCmp('viewMode').disabled && Ext.getCmp('editMode').disabled && Ext.getCmp('RLNO').getValue() != 'LVA' && Ext.getCmp('RLNO').getValue() != 'FVA') {
                                        return Ext.create('Ext.grid.CellEditor', {
                                            field: 'textfield'
                                        });
                                    }
                                    else return false;
                                }
                            },
                            {
                                text: "傳真1",
                                dataIndex: 'FAX_NUMBER',
                                align: 'left',
                                style: 'text-align:center',
                                width: 100,
                                getEditor: function (record) {
                                    if (!Ext.getCmp('viewMode').disabled && Ext.getCmp('editMode').disabled && Ext.getCmp('RLNO').getValue() != 'LVA' && Ext.getCmp('RLNO').getValue() != 'FVA') {
                                        return Ext.create('Ext.grid.CellEditor', {
                                            field: 'textfield'
                                        });
                                    }
                                    else return false;
                                }
                            },
                            {
                                text: "傳真1分機",
                                dataIndex: 'FAX_EXTENS',
                                align: 'left',
                                style: 'text-align:center',
                                width: 100,
                                getEditor: function (record) {
                                    if (!Ext.getCmp('viewMode').disabled && Ext.getCmp('editMode').disabled && Ext.getCmp('RLNO').getValue() != 'LVA' && Ext.getCmp('RLNO').getValue() != 'FVA') {
                                        return Ext.create('Ext.grid.CellEditor', {
                                            field: 'textfield'
                                        });
                                    }
                                    else return false;
                                }
                            },
                            {
                                text: "傳真1備註",
                                dataIndex: 'FAX_MEMO',
                                align: 'left',
                                style: 'text-align:center',
                                autoSizeColumn: true,
                                getEditor: function (record) {
                                    if (!Ext.getCmp('viewMode').disabled && Ext.getCmp('editMode').disabled && Ext.getCmp('RLNO').getValue() != 'LVA' && Ext.getCmp('RLNO').getValue() != 'FVA') {
                                        return Ext.create('Ext.grid.CellEditor', {
                                            field: 'textfield'
                                        });
                                    }
                                    else return false;
                                }
                            },
                            {
                                text: "電傳2",
                                dataIndex: 'FAX_NUMBER2',
                                align: 'left',
                                style: 'text-align:center',
                                width: 100,
                                getEditor: function (record) {
                                    if (!Ext.getCmp('viewMode').disabled && Ext.getCmp('editMode').disabled && Ext.getCmp('RLNO').getValue() != 'LVA' && Ext.getCmp('RLNO').getValue() != 'FVA') {
                                        return Ext.create('Ext.grid.CellEditor', {
                                            field: 'textfield'
                                        });
                                    }
                                    else return false;
                                }
                            },
                            {
                                text: "傳真2分機",
                                dataIndex: 'FAX_EXTENS2',
                                align: 'left',
                                style: 'text-align:center',
                                width: 100,
                                getEditor: function (record) {
                                    if (!Ext.getCmp('viewMode').disabled && Ext.getCmp('editMode').disabled && Ext.getCmp('RLNO').getValue() != 'LVA' && Ext.getCmp('RLNO').getValue() != 'FVA') {
                                        return Ext.create('Ext.grid.CellEditor', {
                                            field: 'textfield'
                                        });
                                    }
                                    else return false;
                                }
                            },
                            {
                                text: "傳真2備註",
                                dataIndex: 'FAX_MEMO2',
                                align: 'left',
                                style: 'text-align:center',
                                autoSizeColumn: true,
                                getEditor: function (record) {
                                    if (!Ext.getCmp('viewMode').disabled && Ext.getCmp('editMode').disabled && Ext.getCmp('RLNO').getValue() != 'LVA' && Ext.getCmp('RLNO').getValue() != 'FVA') {
                                        return Ext.create('Ext.grid.CellEditor', {
                                            field: 'textfield'
                                        });
                                    }
                                    else return false;
                                }
                            },
                            {
                                text: '<span style="color:red" >*</span>EMAIL1',
                                dataIndex: 'SMTP_ADDR',
                                align: 'left',
                                style: 'text-align:center',
                                width: 200,
                                getEditor: function (record) {
                                    if (!Ext.getCmp('viewMode').disabled && Ext.getCmp('editMode').disabled && Ext.getCmp('RLNO').getValue() != 'LVA' && Ext.getCmp('RLNO').getValue() != 'FVA') {
                                        return Ext.create('Ext.grid.CellEditor', {
                                            field: 'textfield'
                                        });
                                    }
                                    else return false;
                                }
                            },
                            {
                                text: "EMAIL1備註",
                                dataIndex: 'SMTP_MEMO',
                                align: 'left',
                                style: 'text-align:center',
                                autoSizeColumn: true,
                                getEditor: function (record) {
                                    if (!Ext.getCmp('viewMode').disabled && Ext.getCmp('editMode').disabled && Ext.getCmp('RLNO').getValue() != 'LVA' && Ext.getCmp('RLNO').getValue() != 'FVA') {
                                        return Ext.create('Ext.grid.CellEditor', {
                                            field: 'textfield'
                                        });
                                    }
                                    else return false;
                                }
                            },
                            {
                                text: "EMAIL2",
                                dataIndex: 'SMTP_ADDR2',
                                align: 'left',
                                style: 'text-align:center',
                                width: 200,
                                getEditor: function (record) {
                                    if (!Ext.getCmp('viewMode').disabled && Ext.getCmp('editMode').disabled && Ext.getCmp('RLNO').getValue() != 'LVA' && Ext.getCmp('RLNO').getValue() != 'FVA') {
                                        return Ext.create('Ext.grid.CellEditor', {
                                            field: 'textfield'
                                        });
                                    }
                                    else return false;
                                }
                            },
                            {
                                text: "EMAIL2備註",
                                dataIndex: 'SMTP_MEMO2',
                                align: 'left',
                                style: 'text-align:center',
                                autoSizeColumn: true,
                                getEditor: function (record) {
                                    if (!Ext.getCmp('viewMode').disabled && Ext.getCmp('editMode').disabled && Ext.getCmp('RLNO').getValue() != 'LVA' && Ext.getCmp('RLNO').getValue() != 'FVA') {
                                        return Ext.create('Ext.grid.CellEditor', {
                                            field: 'textfield'
                                        });
                                    }
                                    else return false;
                                }
                            },
                        ]
                    }
                    //]
                    //},
                ],
                buttons: [
                    {
                        id: 'btnWinSave',
                        disabled: false,
                        text: '儲存',
                        handler: function () {
                            var records = T1Store.getRange();
                            for (var m = 0; m < records.length; m++) {
                                //alert(records[i].data['KTOKK']);
                                if (records[m].data['NAME_LAST'] == '') {
                                    Ext.MessageBox.alert('訊息', '聯絡人項次' + (m + 1) + '，請填寫姓');
                                    return;
                                }
                                if (records[m].data['NAME_FIRST'] == '') {
                                    Ext.MessageBox.alert('訊息', '聯絡人項次' + (m + 1) + '，請填寫名');
                                    return;
                                }
                                if (records[m].data['ABTNR'] == '') {
                                    Ext.MessageBox.alert('訊息', '聯絡人項次' + (m + 1) + '，請填寫部門');
                                    return;
                                }
                                if (records[m].data['PAFKT'] == '') {
                                    Ext.MessageBox.alert('訊息', '聯絡人項次' + (m + 1) + '，請填寫職稱');
                                    return;
                                }
                                if (records[m].data['TEL_NUMBER'] == '') {
                                    Ext.MessageBox.alert('訊息', '聯絡人項次' + (m + 1) + '，請填寫電話1');
                                    return;
                                }
                                if (records[m].data['SMTP_ADDR'] == '') {
                                    Ext.MessageBox.alert('訊息', '聯絡人項次' + (m + 1) + '，請填寫EMAIL1');
                                    return;
                                }
                                if (records[m].data['RMARK'] == '' && records[m].data['PMARK'] == '' && records[m].data['GMARK'] == '') {
                                    Ext.MessageBox.alert('訊息', '聯絡人的三種科目群組記號至少要有一組選擇Y');
                                    return;
                                }

                                //if (records[i].data['PAVIP'] == '1' && records[i].data['RMARK'] == '' && records[i].data['PMARK'] == '' && records[i].data['GMARK'] == '') {
                                //    Ext.MessageBox.alert('訊息', '已選擇為主要聯絡人，則三種科目群組記號至少要有一組選擇Y');
                                //    return;
                                //}

                                var ktokk1 = false, ktokk2 = false, ktokk3 = false;
                                var flag1 = false, flag2 = false, flag3 = false;
                                var records = T3Store.getRange();
                                for (var a = 0; a < records.length; a++) {
                                    if (records[a].data['KTOKK'] == '1ROH')
                                        ktokk1 = true;
                                    else if (records[a].data['KTOKK'] == '2MAT')
                                        ktokk2 = true;
                                    else if (records[a].data['KTOKK'] == '3NOR')
                                        ktokk3 = true;
                                }

                                records = T1Store.getRange();
                                if (ktokk1) {
                                    for (var b = 0; b < records.length; b++) {
                                        if (records[b].data['RMARK'] == 'Y')
                                            flag1 = true;
                                    }
                                }
                                if (ktokk2) {
                                    for (var c = 0; c < records.length; c++) {
                                        if (records[c].data['PMARK'] == 'Y')
                                            flag2 = true;
                                    }
                                }
                                if (ktokk3) {
                                    for (var d = 0; d < records.length; d++) {
                                        if (records[d].data['GMARK'] == 'Y')
                                            flag3 = true;
                                    }
                                }
                                //alert(ktokk1);
                                //alert(flag1);
                                if (ktokk1 ^ flag1) {
                                    Ext.MessageBox.alert('錯誤', "請於[聯絡人清單]'中, 將一位聯絡人之原料記號設定為\"Y\"");
                                    return;
                                }

                                if (ktokk2 ^ flag2) {
                                    Ext.MessageBox.alert('錯誤', "請於[聯絡人清單]'中, 將一位聯絡人之包材記號設定為\"Y\"");
                                    return;
                                }

                                if (ktokk3 ^ flag3) {
                                    Ext.MessageBox.alert('錯誤', "請於[聯絡人清單]'中, 將一位聯絡人之一般物品記號設定為\"Y\"");
                                    return;
                                }
                            }


                            Ext.Msg.confirm('訊息', '確定要修改資料嗎?', function (button) {
                                if (button === 'yes') {
                                    //this.up('window').destroy();
                                    var records = T1Store.getRange();
                                    var data = { item: [] };
                                    for (var i = 0; i < records.length; i++) {
                                        data.item.push(records[i].data);
                                    }
                                    Ext.Ajax.request({
                                        url: '../../../api/SP/SaveContact',
                                        method: reqVal_p,
                                        success: function (response) {
                                            var data = Ext.decode(response.responseText);
                                            if (data.success) {
                                                T1Store.load();
                                                Ext.Msg.alert('訊息', '儲存成功');
                                            }
                                        },
                                        failure: function (response) {
                                            Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                        },
                                        jsonData: data
                                    });
                                }
                            });
                        }
                    },
                    {
                        id: 'winclosed',
                        disabled: false,
                        text: '關閉',
                        handler: function () {
                            this.up('window').destroy();
                        }
                    }
                ]
            });
            if (Ext.getCmp('viewMode').disabled) {
                Ext.getCmp('btnAddContact').setVisible(false);
                Ext.getCmp('btnWinSave').setVisible(false);
                var grid = Ext.ComponentQuery.query('grid')[1];
                var col = grid.getView().getHeaderCt().getHeaderAtIndex(1);
                col.hide();
            }
            else {
                Ext.getCmp('btnAddContact').setVisible(true);
                Ext.getCmp('btnWinSave').setVisible(true);
                var grid = Ext.ComponentQuery.query('grid')[1];
                var col = grid.getView().getHeaderCt().getHeaderAtIndex(1);
                col.show();
            }

            if (Ext.getCmp('RLNO').getValue() == 'LVA' || Ext.getCmp('RLNO').getValue() == 'FVA') {
                Ext.getCmp('btnAddContact').setVisible(false);
                Ext.getCmp('btnWinSave').setVisible(false);
                var grid = Ext.ComponentQuery.query('grid')[1];
                var col = grid.getView().getHeaderCt().getHeaderAtIndex(1);
                col.hide();
                col = grid.getView().getHeaderCt().getHeaderAtIndex(9);
                col.hide();
                col = grid.getView().getHeaderCt().getHeaderAtIndex(10);
                col.hide();
                col = grid.getView().getHeaderCt().getHeaderAtIndex(11);
                col.hide();
                col = grid.getView().getHeaderCt().getHeaderAtIndex(12);
                col.hide();
                col = grid.getView().getHeaderCt().getHeaderAtIndex(13);
                col.hide();
                col = grid.getView().getHeaderCt().getHeaderAtIndex(14);
                col.hide();
            }
                

            var win = GetPopWin(viewport, popform, '聯絡人清單', viewport.width - 250, viewport.height - 20);
        }
        win.show();
    }

    popList = function () {
        if (!win) {
            var T2Tool = Ext.create('Ext.PagingToolbar', {
                store: T2Store,
                //plugins: [{
                //    ptype: "pagesize",
                //    pageSize: 15
                //}],
                displayInfo: true,
                border: false,
                plain: true,
                listeners: {
                    beforechange: function (T2Tool, pageData) {
                        T1Rec = 0; //disable編修按鈕&刪除按鈕
                        T1LastRec = null; //T1Form之資料輸選區清空
                    },
                    afterrender: function (T2Tool) {
                        T2Tool.emptyMsg = '<font color=red>沒有任何資料</font>';
                    }
                }
                //buttons: [
                //    {
                //        itemId: 'add',
                //        text: 'EXCEL',
                //        cls: 'btn-bgStyle insertBtn',
                //        //disabled: true,
                //        handler: function () {
                //        }
                //    }
                //]
            });

            var popform = Ext.create('Ext.form.Panel', {
                height: '100%',
                layout: 'fit',
                closable: false,
                items: [
                    {
                        //xtype: 'fieldset',
                        ////columnWidth: 0.5,
                        //title: '可報價物料清單',
                        //width: 800,
                        //autoScroll: true,
                        //style: "margin:5px;margin-top:30px;background-color: #ecf5ff;",
                        //cls: 'fieldset-title-bigsize',
                        ////cls: 'fieldset-title-red',
                        ////collapsible: false,
                        ////defaultType: 'textfield',
                        ////defaults: { anchor: '70%' },
                        //layout: 'anchor',
                        //items: [
                        //    {
                                xtype: 'grid',
                                store: T2Store,
                                width: 820,
                                dockedItems: [
                                    {
                                        dock: 'top',
                                        xtype: 'toolbar',
                                        autoScroll: true,
                                        items: [T2Tool]
                                    }
                                ],
                                viewConfig: {
                                    listeners: {
                                        // 讓column寬度可以隨著內容自動縮放
                                        refresh: function (dataview) {
                                            Ext.each(dataview.panel.columns, function (column) {
                                                if (column.autoSizeColumn === true)
                                                    column.autoSize();
                                            })
                                        }
                                    }
                                },
                                columns: [
                                    {
                                        xtype: 'rownumberer'
                                    },
                                    {
                                        text: "料號",
                                        dataIndex: 'MATNR',
                                        align: 'center',
                                        style: 'text-align:center',
                                        width: 80,
                                        renderer: function (val, meta, record) {
                                            if (val != null)
                                                //return '<a href="" >' + val + '</a>';
                                                return '<a href="javascript:popMMForm(' + val + ')" >' + val + '</a>';
                                        }
                                    },
                                    {
                                        text: "品名",
                                        dataIndex: 'MAKTX',
                                        align: 'left',
                                        style: 'text-align:center',
                                        autoSizeColumn: true
                                    },
                                    {
                                        text: "製造商ID",
                                        dataIndex: 'LTSNR',
                                        align: 'left',
                                        style: 'text-align:center',
                                        //autoSizeColumn: true,
                                        width: 80,
                                        renderer: function (val, meta, record) {
                                            if (val != null)
                                                //return '<a href="" >' + val + '</a>';
                                                return '<a href="javascript:popMFForm(' + val + ')" >' + val + '</a>';
                                        }
                                    },
                                    {
                                        text: "製造商名稱",
                                        dataIndex: 'MFRNC',
                                        align: 'left',
                                        style: 'text-align:center',
                                        autoSizeColumn: true,
                                        //renderer: function (val, meta, record) {
                                        //    if (val != null)
                                        //        return '<a href="" >' + val + '</a>';
                                        //}
                                    },
                                    //{
                                    //    text: "代工廠名稱",
                                    //    tdCls: 'wrap-text',
                                    //    dataIndex: 'OEMNC',
                                    //    align: 'left',
                                    //    style: 'text-align:center',
                                    //    width: 150
                                    //},
                                    //{
                                    //    text: "供應商名稱",
                                    //    dataIndex: 'NAME1',
                                    //    align: 'left',
                                    //    style: 'text-align:center',
                                    //    autoSizeColumn: true
                                    //}
                                ]
                        //    }
                        //]
                    },
                ],
                buttons: [{
                    id: 'winclosed',
                    disabled: false,
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
            });
            var win = GetPopWin(viewport, popform, '可報價物料清單', viewport.width - 250, viewport.height - 20);
        }
        win.show();
    }

    popLicense = function (param) {
        var strUrl = '';
        if (!Ext.getCmp('viewMode').disabled && Ext.getCmp('editMode').disabled)
            strUrl = 'SP_POP_LICENSE?code=' + param + '&edit=1';
        else
            strUrl = 'SP_POP_LICENSE?code=' + param + '&edit=0';
        if (!win) {
            var popform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                height: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + strUrl + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0"  style="background-color:#FFFFFF"></iframe>',
                //buttons: [{ xtype: 'label', id: 'DivResult', text: '上傳結果', width: '50%', color: 'blue' },
                buttons: [
                    {
                        id: 'winclosed',
                        disabled: false,
                        text: '關閉',
                        handler: function () {
                            this.up('window').destroy();
                        }
                    }
                ]
            });
            var win = GetPopWin(viewport, popform, '供應商證照（許可執照字號/公司登記/工廠登記….）', viewport.width - 20, viewport.height - 20);
        }
        win.show();
    }

    popRecord = function () {
        //alert(T1Form.getForm().findField('TMPKEY').getValue());
        if (!win) {
            var popform = Ext.create('Ext.form.Panel', {
                height: '100%',
                layout: 'fit',
                closable: false,
                items: [
                    {
                        xtype: 'grid',
                        //title: '引用製造商清單',
                        store: T8Store,
                        //viewConfig: {
                        //    listeners: {
                        //        // 讓column寬度可以隨著內容自動縮放
                        //        refresh: function (dataview) {
                        //            Ext.each(dataview.panel.columns, function (column) {
                        //                if (column.autoSizeColumn === true)
                        //                    column.autoSize();
                        //            })
                        //        }
                        //    }
                        //},
                        selModel: {
                            selType: 'cellmodel'
                        },
                        columns: [
                            {
                                xtype: 'rownumberer'
                            },
                            {
                                text: "送出者",
                                dataIndex: 'SENDER',
                                align: 'center',
                                style: 'text-align:center',
                                width: 80,
                            },
                            {
                                text: "接收者",
                                dataIndex: 'RECEIVER',
                                align: 'center',
                                style: 'text-align:center',
                                //autoSizeColumn: true,
                                width: 80,
                            },
                            {
                                text: "動作",
                                dataIndex: 'ACTION',
                                align: 'left',
                                style: 'text-align:center',
                                //autoSizeColumn: true,
                                width: 100,
                            },
                            {
                                text: "意見",
                                dataIndex: 'COMMENT',
                                align: 'left',
                                style: 'text-align:center',
                                //autoSizeColumn: true,
                                width: 200,
                            },
                            {
                                text: "日期",
                                dataIndex: 'DATE',
                                align: 'left',
                                style: 'text-align:center',
                                width: 170,
                            }

                        ]
                    }
                    //]
                    //},
                ],
                buttons: [{
                    id: 'winclosed',
                    disabled: false,
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
            });

            var win = GetPopWin(viewport, popform, '簽核歷程', viewport.width - 250, viewport.height - 20);
        }
        win.show();
    }

    popMMForm = function (param) {
        var src = '../Form/Index/MM/MM01_detail?matnr=' + param;
        var tp = window.parent.tabs;
        //如果tab已存在，則跳到該tab
        var target = tp.child('[title=' + '物料' + param + ']');
        if (target != null) {
            tp.setActiveTab(target);
        }
        else {
            var iframe1 = document.createElement("IFRAME");
            iframe1.id = "frame" + param;
            iframe1.frameBorder = 0;
            iframe1.src = src;
            iframe1.height = "100%";
            iframe1.width = "100%";
            var tabItem = {
                title: '物料' + param,
                id: 'tab' + param,
                itemId: "tab" + param,
                html: '<iframe src="' + src + '" id="frame' + param + '" width="100%" height="100%" frameborder="0"></iframe>',
                closable: true
            };
            var newTab = tp.add(tabItem);
            tp.setActiveTab(newTab);
        }
    }

    popMFForm = function (param) {
        var src = '../Form/Index/MF/MF01_detail?code=' + param;
        var tp = window.parent.tabs;
        //如果tab已存在，則跳到該tab
        var target = tp.child('[title=' + '製造商' + param + ']');
        if (target != null) {
            tp.setActiveTab(target);
        }
        else {
            var iframe1 = document.createElement("IFRAME");
            iframe1.id = "frame" + param;
            iframe1.frameBorder = 0;
            iframe1.src = src;
            iframe1.height = "100%";
            iframe1.width = "100%";
            var tabItem = {
                title: '製造商' + param,
                id: 'tab' + param,
                itemId: "tab" + param,
                html: '<iframe src="' + src + '" id="frame' + param + '" width="100%" height="100%" frameborder="0"></iframe>',
                closable: true
            };
            var newTab = tp.add(tabItem);
            tp.setActiveTab(newTab);
        }
    }

    function reloadFileList() {
        var rtn = '';
        Ext.Ajax.request({
            url: '../../../api/SP/GetFileList',
            method: reqVal_p,
            params: {
                //p0: record.data['MFID1'],
                p1: mlifnr,
                p2: 'SP01_DETAIL'
            },
            async: false,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    for (var i = 0; i < data.etts.length; i++) {
                        rtn += '<a href="javascript:DownloadFile(\'' + data.etts[i].FG + '\')" >' + data.etts[i].FN + '</a> <br>';
                    }
                }
            },
            failure: function (response) {
                Ext.MessageBox.alert('錯誤', '發生例外錯誤');
            }
        });
        Ext.getCmp('ATTACHMENT').setValue(rtn);
    }

    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true
        },
        items: [{
            //itemId: 'form',
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [T1Form]
        }]
    });

    T1Load();
});