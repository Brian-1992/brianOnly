Ext.Loader.loadScript({ url: location.pathname.substring(0, location.pathname.indexOf('Form')) + 'Scripts/SP/SP02_detail_store.js' });
Ext.onReady(function () {
    Ext.QuickTips.init();
    //var T1Store = Ext.create('Ext.data.Store', {
    //    fields: ['id', 'name', 'taxid', 'contactname', 'address', 'status'],
    //    data: [
    //        { id: '100001', name: 'XXX股份有限公司', contactname: 'John', address: '台中市大甲區中山路一段1191號', status: '草稿' },
    //        { id: '100002', name: 'YYY股份有限公司', contactname: 'Andy', address: '台中市大甲區中山路一段1191號', status: '審核中' },
    //        { id: '100003', name: 'ZZZ股份有限公司', contactname: 'Jasmine', address: '台中市大甲區中山路一段1191號', status: '審核中' },
    //        { id: '100004', name: 'XYZ股份有限公司', contactname: 'Jacky', address: '台中市大甲區中山路一段1191號', status: '同意' }
    //    ]
    //});

    //var T2Store = Ext.create('Ext.data.Store', {
    //    fields: ['meterialno', 'name', 'groupid', 'vendorname', 'status'],
    //    data: [
    //        { meterialno: '1111361', name: 'Azithromycin hydrate, JP', groupid: '11111', vendorname: 'Health Creation Pharma Limited', status: '草稿' },
    //        { meterialno: '1159640', name: 'Azithromycin hydrate', groupid: '22222', vendorname: 'Health Creation Pharma Limited', status: '草稿' },
    //        { meterialno: '2078163', name: 'CASE VGO 40GM', groupid: '33333', vendorname: '豪門彩色印刷事業(股) 公司', status: '草稿' },
    //    ]
    //});

    //var T3Store = Ext.create('Ext.data.Store', {
    //    fields: ['id', 'name', 'tel', 'ext', 'mobile', 'email', 'job', 'note'],
    //    data: [
    //        { id: '100001', name: '楊玉萍', tel: '06-2211998', ext: '202', mobile: '88662211998202', email: 'apple@chongdah.com.tw', job: '', note: '' },
    //        { id: '100002', name: '蔡珮俞', tel: '06-2211998', ext: '205', mobile: '88662211998205', email: 'cindytsai@chongdah.com.tw', job: '業務專員', note: '產假+孕嬰假(2018/1/22-7/31)' },
    //        { id: '100003', name: '葉馨黛', tel: '06-2211998', ext: '222', mobile: '88662211998222', email: '', job: '經理', note: '專線' }
    //    ]
    //});

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
    }

    var topTool = Ext.create('Ext.toolbar.Toolbar', {
        border: 0,
        items: [
            {
                text: '聯絡人清單',
                cls: 'funBtn',
                handler: function () {
                    popContact();
                }
            }, '-',
            //{
            //    text: '可報價物料清單',
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
        listeners: {
            beforeedit: function (editor, e, eOpts) {
                if (Ext.getCmp('RLNO').getValue() == 'PGRM') {
                    return false;  // 取消cell editing模式
                }
            }
        }
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
                                xtype: 'displayfield',
                                id: 'NAME1',
                                fieldStyle: 'font-weight:bold;',
                                fieldLabel: '供應商名稱(中)',
                                labelAlign: 'right',
                                labelWidth: 140,
                                //allowBlank: false
                            },
                            {
                                xtype: 'displayfield',
                                id: 'TMP_NAME1',
                                name: 'TMP_NAME1',
                                fieldStyle: 'font-weight:bold;color:red;',
                                labelStyle: 'color:red;',
                                fieldLabel: '異動後供應商名稱(中)',
                                labelAlign: 'right',
                                labelWidth: 140,
                                width: 600
                                //allowBlank: false
                            },
                            {
                                xtype: 'displayfield',
                                id: 'NAME2',
                                fieldStyle: 'font-weight:bold;',
                                fieldLabel: '供應商名稱(英)',
                                labelAlign: 'right',
                                labelWidth: 140,
                                //allowBlank: false
                            },
                            {
                                xtype: 'displayfield',
                                id: 'TMP_NAME2',
                                name: 'TMP_NAME2',
                                fieldStyle: 'font-weight:bold;color:red;',
                                labelStyle: 'color:red;',
                                fieldLabel: '異動後供應商名稱(英)',
                                labelAlign: 'right',
                                labelWidth: 140,
                                width: 600
                            },
                            {
                                xtype: 'displayfield',
                                id: 'TEL_NUMBER',
                                name: 'TEL_NUMBER',
                                fieldStyle: 'font-weight:bold;',
                                fieldLabel: '公司電話',
                                labelAlign: 'right',
                                labelWidth: 140,
                                width: 600
                            },
                            {
                                xtype: 'displayfield',
                                id: 'FAX_NUMBER',
                                name: 'FAX_NUMBER',
                                fieldStyle: 'font-weight:bold;',
                                fieldLabel: '公司傳真',
                                labelAlign: 'right',
                                labelWidth: 140,
                                width: 600
                            },
                            {
                                xtype: 'displayfield',
                                id: 'SMTP_ADDR',
                                name: 'SMTP_ADDR',
                                fieldStyle: 'font-weight:bold;',
                                fieldLabel: '公司Email',
                                labelAlign: 'right',
                                labelWidth: 140,
                                width: 600,
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
                                xtype: 'displayfield',
                                id: 'URL',
                                name: 'URL',
                                fieldStyle: 'font-weight:bold;',
                                fieldLabel: '公司網址',
                                labelAlign: 'right',
                                labelWidth: 140,
                                width: 600
                            },
                            {
                                xtype: 'displayfield',
                                id: 'ADDR',
                                name: 'ADDR',
                                fieldStyle: 'font-weight:bold;',
                                fieldLabel: '公司地址',
                                labelAlign: 'right',
                                labelWidth: 140,
                                width: 600
                            },
                            {
                                xtype: 'displayfield',
                                id: 'TMP_ADDR',
                                name: 'TMP_ADDR',
                                fieldStyle: 'font-weight:bold;color:red;',
                                labelStyle: 'color:red;',
                                fieldLabel: '異動後公司地址',
                                labelAlign: 'right',
                                labelWidth: 140,
                                width: 600
                            },
                            {
                                xtype: 'displayfield',
                                id: 'POST_CODE1',
                                name: 'POST_CODE1',
                                fieldStyle: 'font-weight:bold;',
                                fieldLabel: '郵遞區號',
                                labelAlign: 'right',
                                labelWidth: 140,
                                width: 600
                            },
                            {
                                xtype: 'displayfield',
                                id: 'J_1KFREPRE',
                                name: 'J_1KFREPRE',
                                fieldStyle: 'font-weight:bold;',
                                fieldLabel: '負責人',
                                labelAlign: 'right',
                                labelWidth: 140,
                                width: 600
                            },
                            {
                                xtype: 'displayfield',
                                id: 'ADDRC',
                                name: 'ADDRC',
                                fieldStyle: 'font-weight:bold;',
                                fieldLabel: '通訊地址',
                                labelAlign: 'right',
                                labelWidth: 140,
                                width: 600
                            },
                            {
                                xtype: 'displayfield',
                                id: 'ADDRS',
                                name: 'ADDRS',
                                fieldStyle: 'font-weight:bold;',
                                fieldLabel: '倉庫地址',
                                labelAlign: 'right',
                                labelWidth: 140,
                                width: 600
                            },
                            {
                                xtype: 'displayfield',
                                id: 'CORPORATION',
                                name: 'CORPORATION',
                                fieldStyle: 'font-weight:bold;',
                                fieldLabel: '供應商母公司',
                                labelAlign: 'right',
                                labelWidth: 140,
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
                //columnWidth: 0.5,
                title: '採購經辦維護資料',
                autoHeight: true,
                width: 800,
                style: "background-color: #ecf5ff;",
                cls: 'fieldset-title-bigsize',
                //collapsible: false,
                //defaultType: 'textfield',
                //defaults: { anchor: '70%' },
                layout: 'anchor',
                items: [
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
                        sortableColumns: false,
                        store: T3Store,
                        height: 140,
                        //plugins: [rowEditing],
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
                                text: "科目群組",
                                dataIndex: 'KTOKK',
                                align: 'left',
                                style: 'text-align:center',
                                width: 80,
                                //locked: true,
                                sortable: false,
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
                                        return Ext.create('Ext.grid.CellEditor', {
                                            field: Ext.create('Ext.form.field.ComboBox', {
                                                //forceSelection: true,
                                                //store: [[1, 'Option 1'], [2, 'Option 2']]
                                                displayField: 'TEXT1',
                                                valueField: 'KTOKK',
                                                store: ktokkStore,
                                            })
                                        });
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
                                sortable: false,
                            },
                            {
                                text: "付款條件",
                                dataIndex: 'ZTERM',
                                align: 'left',
                                style: 'text-align:center',
                                width: 400,
                                //locked: true,
                                sortable: false,
                                editor: {
                                    xtype: 'combo',
                                    displayField: 'TEXT1',
                                    valueField: 'ZTERM',
                                    store: T7Store
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
                                text: '<span style="color: red">異動後付款條件</span>',
                                dataIndex: 'TMP_ZTERM',
                                align: 'left',
                                style: 'text-align:center',
                                width: 400,
                                //locked: true,
                                sortable: false,
                                //editor: {
                                //    xtype: 'combo',
                                //    displayField: 'TEXT1',
                                //    valueField: 'ZTERM',
                                //    store: T7Store
                                //},
                                renderer: function (value, metaData, record) {
                                    var rtn;
                                    //var zterm = record.data['ZTERM'].split('-');
                                    Ext.Ajax.request({
                                        url: '../../../api/NM/GetZterm',
                                        method: reqVal_p,
                                        params: {
                                            p0: record.data['TMP_ZTERM']
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
                                text: "付款方式",
                                dataIndex: 'ZWELS',
                                align: 'left',
                                style: 'text-align:center',
                                sortable: false,
                                width: 150,
                                editor: {
                                    xtype: 'combo',
                                    displayField: 'TEXT1',
                                    valueField: 'ZLSCH',
                                    store: T5Store
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
                                text: '<span style="color: red">異動後付款方式</span>',
                                dataIndex: 'TMP_ZWELS',
                                align: 'left',
                                style: 'text-align:center',
                                sortable: false,
                                width: 150,
                                renderer: function (value, metaData, record) {
                                    var rtn;
                                    Ext.Ajax.request({
                                        url: '../../../api/NM/GetZwels',
                                        method: reqVal_p,
                                        params: {
                                            p0: record.data['TMP_ZWELS']
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
                                text: "訂單貨幣",
                                dataIndex: 'WAERS',
                                align: 'left',
                                style: 'text-align:center',
                                sortable: false,
                                width: 80,
                                editor: {
                                    xtype: 'combo',
                                    displayField: 'WAERS',
                                    valueField: 'WAERS',
                                    store: T11Store
                                }
                            },
                            {
                                text: "國貿條件1",
                                dataIndex: 'INCO1',
                                align: 'left',
                                style: 'text-align:center',
                                sortable: false,
                                width: 180,
                                editor: {
                                    xtype: 'combo',
                                    displayField: 'TEXT1',
                                    valueField: 'INCO1',
                                    store: T6Store
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
                                text: "國貿條件2",
                                dataIndex: 'INCO2',
                                align: 'left',
                                style: 'text-align:center',
                                sortable: false,
                                width: 200,
                                editor: {
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
                                sortable: false,
                                width: 70,
                                editor: {
                                    xtype: 'checkbox'
                                }
                            },
                            {
                                text: "刪除",
                                dataIndex: 'MARK2',
                                align: 'left',
                                style: 'text-align:center',
                                sortable: false,
                                width: 70,
                                editor: {
                                    xtype: 'checkbox'
                                }
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
                    {
                        xtype: 'button',
                        id: 'btnAccept',
                        text: '同意',
                        //iconCls: 'MGRPClear',
                        handler: function () {
                            Ext.Msg.confirm('訊息', '確定要審核通過嗎?', function (button) {
                                if (button === 'yes') {
                                    T1Submit(1, '');

                                }
                            });
                        }
                    },
                    {
                        xtype: 'button',
                        id: 'btnBack',
                        text: '退回',
                        //iconCls: 'MGRPClear',
                        handler: function () {
                            popBack();
                        }
                    }
                ]
            }
        ]
    });

    function T1Submit(status_code, comment) {
        Ext.Ajax.request({
            url: '../../../api/SP/UpdateStatus',
            method: reqVal_p,
            params: {
                code: status_code,
                konzs: Ext.getCmp('KONZS1').getValue(),
                comment: comment
            },
            //async: true,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    //Ext.MessageBox.alert('訊息', '申請單送出成功');
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

    var rowEditingContact = Ext.create('Ext.grid.plugin.RowEditing', {
        //clicksToMoveEditor: 1,
        clicksToEdit: 1,
        autoCancel: false,
        saveBtnText: '更新',
        cancelBtnText: '取消',
        listeners: {
            beforeedit: function (editor, context, eOpts) {
                if (Ext.getCmp('RLNO').getValue() == 'PGRM') {
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
                        xtype: 'grid',
                        height: 400,
                        //title: '引用製造商清單',
                        store: T1Store,
                        sortableColumns: false,
                        id: 'grid_contact',
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
                        listeners: {
                            'itemcontextmenu': function (grid, record, item, index, e, eOpts) {
                                e.preventDefault(); //先取消瀏覽器上的右鍵事件處理
                                grid.getSelectionModel().select(record);
                                //contextmenu.showAt(e.getXY());
                                // 已經有user id的則不顯示刪除
                                if (record.data['USERID'] == null || record.data['USERID'] == '')
                                    popContextMenu_contact(e.getXY(), record, T1Store);
                            }
                        },
                        columns: [
                            {
                                xtype: 'rownumberer'
                            },
                            {
                                text: "姓",
                                dataIndex: 'NAME_LAST',
                                align: 'left',
                                style: 'text-align:center',
                                width: 80,
                                //locked: true,
                                sortable: false,
                                editor: {}
                            },
                            {
                                text: "名",
                                dataIndex: 'NAME_FIRST',
                                align: 'left',
                                style: 'text-align:center',
                                width: 80,
                                //locked: true,
                                sortable: false,
                                editor: {}
                            },
                            {
                                text: "USERID",
                                dataIndex: 'USERID',
                                align: 'center',
                                style: 'text-align:center',
                                width: 80,
                                //locked: true,
                                sortable: false
                            },
                            {
                                text: "VIP",
                                dataIndex: 'PAVIP',
                                align: 'center',
                                style: 'text-align:center',
                                width: 50,
                                editor: {
                                    xtype: 'combo',
                                    displayField: 'name',
                                    valueField: 'value',
                                    store: vipStore
                                }
                            },
                            {
                                text: "部門",
                                dataIndex: 'ABTNR',
                                align: 'left',
                                style: 'text-align:center',
                                sortable: false,
                                width: 230,
                                editor: {
                                    xtype: 'combo',
                                    displayField: 'TEXT1',
                                    valueField: 'ABTNR',
                                    store: T9Store
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
                                text: "職稱",
                                dataIndex: 'PAFKT',
                                align: 'left',
                                style: 'text-align:center',
                                sortable: false,
                                width: 80,
                                editor: {
                                    xtype: 'combo',
                                    displayField: 'TEXT1',
                                    valueField: 'PAFKT',
                                    store: T10Store
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
                                editor: {}
                            },
                            {
                                text: "電話1",
                                dataIndex: 'TEL_NUMBER',
                                align: 'left',
                                style: 'text-align:center',
                                width: 100,
                                editor: {}
                            },
                            {
                                text: "分機1",
                                dataIndex: 'TEL_EXTENS',
                                align: 'left',
                                style: 'text-align:center',
                                width: 60,
                                editor: {}
                            },
                            {
                                text: "電話1備註",
                                dataIndex: 'TEL_MEMO',
                                align: 'left',
                                style: 'text-align:center',
                                autoSizeColumn: true,
                                editor: {}
                            },
                            {
                                text: "電話2",
                                dataIndex: 'TEL_NUMBER2',
                                align: 'left',
                                style: 'text-align:center',
                                width: 100,
                                editor: {}
                            },
                            {
                                text: "分機2",
                                dataIndex: 'TEL_EXTENS2',
                                align: 'left',
                                style: 'text-align:center',
                                width: 100,
                                editor: {}
                            },
                            {
                                text: "電話2備註",
                                dataIndex: 'TEL_MEMO2',
                                align: 'left',
                                style: 'text-align:center',
                                autoSizeColumn: true,
                                editor: {}
                            },
                            {
                                text: "行動電話1",
                                dataIndex: 'MOB_NUMBER',
                                align: 'left',
                                style: 'text-align:center',
                                width: 100,
                                editor: {}
                            },
                            {
                                text: "行動電話1備註",
                                dataIndex: 'MOB_MEMO',
                                align: 'left',
                                style: 'text-align:center',
                                autoSizeColumn: true,
                                editor: {}
                            },

                            {
                                text: "行動電話2",
                                dataIndex: 'MOB_NUMBER2',
                                align: 'left',
                                style: 'text-align:center',
                                width: 100,
                                editor: {}
                            },
                            {
                                text: "行動電話2備註",
                                dataIndex: 'MOB_MEMO2',
                                align: 'left',
                                style: 'text-align:center',
                                autoSizeColumn: true,
                                editor: {}
                            },
                            {
                                text: "傳真1",
                                dataIndex: 'FAX_NUMBER',
                                align: 'left',
                                style: 'text-align:center',
                                width: 100,
                                editor: {}
                            },
                            {
                                text: "傳真1分機",
                                dataIndex: 'FAX_EXTENS',
                                align: 'left',
                                style: 'text-align:center',
                                width: 100,
                                editor: {}
                            },
                            {
                                text: "傳真1備註",
                                dataIndex: 'FAX_MEMO',
                                align: 'left',
                                style: 'text-align:center',
                                autoSizeColumn: true
                            },
                            {
                                text: "電傳2",
                                dataIndex: 'FAX_NUMBER2',
                                align: 'left',
                                style: 'text-align:center',
                                width: 100,
                                editor: {}
                            },
                            {
                                text: "傳真2分機",
                                dataIndex: 'FAX_EXTENS2',
                                align: 'left',
                                style: 'text-align:center',
                                width: 100,
                                editor: {}
                            },
                            {
                                text: "傳真2備註",
                                dataIndex: 'FAX_MEMO2',
                                align: 'left',
                                style: 'text-align:center',
                                autoSizeColumn: true,
                                editor: {}
                            },
                            {
                                text: "EMAIL1",
                                dataIndex: 'SMTP_ADDR',
                                align: 'left',
                                style: 'text-align:center',
                                width: 200,
                                editor: {}
                            },
                            {
                                text: "EMAIL2",
                                dataIndex: 'SMTP_ADDR2',
                                align: 'left',
                                style: 'text-align:center',
                                width: 200,
                                editor: {}
                            },
                            {
                                text: "原料記號",
                                dataIndex: 'RMARK',
                                align: 'left',
                                style: 'text-align:center',
                                width: 70,
                                editor: {
                                    xtype: 'combo',
                                    displayField: 'name',
                                    valueField: 'value',
                                    store: ynStore
                                }
                            },
                            {
                                text: "包材記號",
                                dataIndex: 'PMARK',
                                align: 'left',
                                style: 'text-align:center',
                                width: 70,
                                editor: {
                                    xtype: 'combo',
                                    displayField: 'name',
                                    valueField: 'value',
                                    store: ynStore
                                }
                            },
                            {
                                text: "一般物品記號",
                                dataIndex: 'GMARK',
                                align: 'left',
                                style: 'text-align:center',
                                width: 90,
                                editor: {
                                    xtype: 'combo',
                                    displayField: 'name',
                                    valueField: 'value',
                                    store: ynStore
                                }
                            },
                            {
                                text: "登入系統",
                                dataIndex: 'SYS',
                                align: 'left',
                                style: 'text-align:center',
                                width: 70,
                                editor: {
                                    xtype: 'combo',
                                    displayField: 'name',
                                    valueField: 'value',
                                    store: ynStore
                                }
                            },
                            {
                                text: "管理者",
                                dataIndex: 'ADMIN1',
                                align: 'left',
                                style: 'text-align:center',
                                width: 50,
                                editor: {
                                    xtype: 'combo',
                                    displayField: 'name',
                                    valueField: 'value',
                                    store: ynStore
                                }
                            },
                            {
                                text: "公開",
                                dataIndex: 'OPEN1',
                                align: 'left',
                                style: 'text-align:center',
                                width: 50,
                                editor: {
                                    xtype: 'combo',
                                    displayField: 'name',
                                    valueField: 'value',
                                    store: ynStore
                                }
                            },
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

            var win = GetPopWin(viewport, popform, '聯絡人清單', viewport.width - 250, viewport.height - 20);
        }
        win.show();
    }

    popList = function () {
        if (!win) {
            var T2Tool = Ext.create('Ext.PagingToolbar', {
                store: T2Store,
                plugins: [{
                    ptype: "pagesize",
                    pageSize: 15
                }],
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
        //if (Ext.getCmp('editMode').disabled)
        //    strUrl = 'SP_POP_LICENSE?code=' + param + '&edit=1';
        //else
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

    popBack = function () {
        //alert(T1Form.getForm().findField('TMPKEY').getValue());
        if (!win) {
            var popform = Ext.create('Ext.form.Panel', {
                //height: '100%',
                //layout: 'fit',
                width: 300,
                height: 300,
                bodyPadding: 10,
                closable: false,
                autoScroll: true,
                items: [
                    {
                        xtype: 'textareafield',
                        id: 'backReason',
                        grow: true,
                        name: 'message',
                        fieldLabel: '退回原因',
                        anchor: '70%'
                    }
                ],
                buttons: [
                    {
                        //id: 'winclosed',
                        disabled: false,
                        text: '退回',
                        handler: function () {
                            Ext.Msg.confirm('訊息', '確定要退回嗎?', function (button) {
                                if (button === 'yes') {
                                    T1Submit(5, Ext.getCmp('backReason').getValue());
                                    //alert(Ext.getCmp('RLNO').getValue());
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

            var win = GetPopWin(viewport, popform, '退回', viewport.width - 350, viewport.height - 300);
        }
        win.show();
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