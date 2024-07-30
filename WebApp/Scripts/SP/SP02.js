Ext.Loader.loadScript({ url: location.pathname.substring(0, location.pathname.indexOf('Form')) + 'Scripts/SP/SP02_store.js' });
Ext.onReady(function () {
    var T1Name = "新會員申請";
    var T1Rec = 0;
    var T1LastRec = null;
    var GridReClick = false;
    var isClick = false;
    var autModify = "";
    var confirmSubmit = "";
    var pSize = 20; //分頁時每頁筆數

    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    };
    var showgridname = Ext.getUrlParam('strParam');
    if (showgridname == "showGRIDNAME") {
        top.Ext.getCmp('tabMain').setTitle("首頁");
    }
    else
        T1Name = "";

    var mLabelWidth = 90;
    var mWidth = 210;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true,
        fieldDefaults: {
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [
            {
                xtype: 'panel',
                id: 'PanelP3',
                border: false,
                layout: 'hbox',
                items: [
                    {
                        xtype: 'container',
                        //layout: {
                        //    type: 'vbox',
                        //    align: 'center',
                        //    pack: 'center'
                        //},             
                        layout: {
                            type: 'table',
                            columns: 2
                        },
                        items: [
                            {
                                xtype: 'textfield',
                                id: 'konzs',
                                fieldLabel: '供應商編號',
                                editable: false,
                                labelAlign: 'right'
                            },
                            {
                                xtype: 'textfield',
                                id: 'stceg',
                                fieldLabel: '統一編號',
                                editable: false,
                                labelAlign: 'right'
                            },
                            {
                                xtype: 'textfield',
                                id: 'name1',
                                fieldLabel: '供應商名稱(中)',
                                editable: false,
                                labelAlign: 'right'
                            },
                            {
                                xtype: 'textfield',
                                id: 'name2',
                                fieldLabel: '供應商名稱(英)',
                                editable: false,
                                labelAlign: 'right'
                            },
                            {
                                xtype: 'hiddenfield',
                                id: 'RLNO',
                                name: 'RLNO',
                                value: ''
                            },
                        ]
                    }
                ]
            },
            {
                xtype: 'panel',
                id: 'PanelP4',
                border: false,
                layout: 'hbox',
                items: [
                    {
                        xtype: 'button',
                        text: '查詢',
                        iconCls: 'TRASearch',
                        handler: function () {
                            //clearEmptyMsg();
                            T1Load();
                        }
                    }, {
                        xtype: 'button',
                        text: '清除',
                        iconCls: 'TRAClear',
                        handler: function () {
                            var f = this.up('form').getForm();
                            f.reset();
                            //f.findField('T1P0').focus();
                            T2Store.load();
                        }
                    }]
            }]
    });

    function T1Load() {
        T1Store.load({ params: { start: 0 } });
        T2Store.load({ params: { start: 0 } });
    }

    //var T1Store = Ext.create('Ext.data.Store', {
    //    fields: ['id', 'name', 'taxid', 'contactname', 'address', 'status', 'country'],
    //    data: [
    //        { id: '100001', name: 'XXX股份有限公司', taxid: '88881111', contactname: 'John', address: '台中市大甲區中山路一段1191號', status: '草稿', country: 'TW' },
    //        { id: '100002', name: 'YYY股份有限公司', taxid: '00001111', contactname: 'Andy', address: '台中市大甲區中山路一段1191號', status: '同意', country: 'TW' },
    //        { id: '100003', name: 'ZZZ股份有限公司', taxid: '22223333', contactname: 'Jasmine', address: '台中市大甲區中山路一段1191號', status: '審核中', country: 'TW' },
    //        { id: '100004', name: 'XYZ股份有限公司', taxid: '44447777', contactname: 'Jacky', address: '台中市大甲區中山路一段1191號', status: '不同意', country: 'TW' }
    //    ]
    //});

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        //plugins: [{
        //    ptype: "pagesize",
        //    pageSize: pSize
        //}],
        displayInfo: true,
        border: false,
        plain: true,
        listeners: {
            beforechange: function (T1Tool, pageData) {
                T1Rec = 0; //disable編修按鈕&刪除按鈕
                T1LastRec = null; //T1Form之資料輸選區清空
            },
            afterrender: function (T1Tool) {
                T1Tool.emptyMsg = '<font color=red>沒有任何資料</font>';
            }
        },
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

    var T1Grid = Ext.create('Ext.grid.Panel', {
        title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            autoScroll: true,
            items: [T1Tool]
        }],
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "審核狀態",
                dataIndex: 'STATUS',
                align: 'left',
                style: 'text-align:center',
                width: 60,
                renderer: function (val, meta, record) {
                    if (val != null) {
                        if (T1Query.getForm().findField('RLNO').getValue() == 'ADMG')
                            return '<a href="javascript:popWinForm(' + record.data['KONZS'] + ",'" + val + "')\" >" + val + '</a>';
                        else if (T1Query.getForm().findField('RLNO').getValue() == 'PGR' && val == '經辦審核中'
                            || T1Query.getForm().findField('RLNO').getValue() == 'PGR' && val == '主管退回')
                            return '<a href="javascript:popWinForm(' + record.data['KONZS'] + ",'" + val + "')\" >" + val + '</a>';
                        else if (T1Query.getForm().findField('RLNO').getValue() == 'PGRM' && val == '主管審核中')
                            return '<a href="javascript:popWinForm(' + record.data['KONZS'] + ",'" + val + "')\" >" + val + '</a>';
                        else
                            return val;
                    }
                }
            },
            {
                text: "供應商名稱",
                dataIndex: 'NAME1',
                align: 'left',
                style: 'text-align:center',
                autoSizeColumn: true
            },
            {
                text: "供應商名稱(英)",
                dataIndex: 'NAME2',
                align: 'left',
                style: 'text-align:center',
                autoSizeColumn: true
            },
            {
                text: "統一編號",
                dataIndex: 'STCEG',
                align: 'center',
                style: 'text-align:center',
                autoSizeColumn: true
            },
            {
                text: "公司地址",
                dataIndex: 'ADDR',
                align: 'left',
                style: 'text-align:center',
                autoSizeColumn: true
            },
            {
                text: "原料記號",
                dataIndex: 'RMARK',
                align: 'left',
                style: 'text-align:center',
                autoSizeColumn: true
            },
            {
                text: "包材記號",
                dataIndex: 'PMARK',
                align: 'left',
                style: 'text-align:center',
                autoSizeColumn: true
            },
            {
                text: "一般物品記號",
                dataIndex: 'GMARK',
                align: 'left',
                style: 'text-align:center',
                autoSizeColumn: true
            }
        ],
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
            }
        }
    });

    popWinForm = function (param, val) {
        //if (!win) {
        //    var popform = Ext.create('Ext.form.Panel', {
        //        id: 'iframeReport',
        //        height: '100%',
        //        layout: 'fit',
        //        closable: false,
        //        html: '<iframe src="./SP02_detail" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0"  style="background-color:#FFFFFF"></iframe>',
        //        buttons: [{
        //            id: 'winclosed',
        //            disabled: false,
        //            text: '關閉',
        //            handler: function () {
        //                this.up('window').destroy();
        //            }
        //        }]
        //    });
        //    var win = GetPopWin(viewport, popform, '供應商詳細資料', viewport.width - 20, viewport.height - 20);
        //}
        //win.show();

        var src = '../Form/Index/SP/SP02_detail?mlifnr=' + param;

        Ext.Ajax.request({
            url: '../../../api/SP/CheckEditor',
            method: reqVal_p,
            params: {
                konzs: param
            },
            //async: true,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    if (data.msg == '1' && val == '主管退回') 
                        src = '../Form/Index/SP/SP01_detail?mlifnr=' + param;


                    var tp = window.parent.tabs;
                    var activeTab = tp.getActiveTab();
                    //alert(activeTab.id);
                    //alert(Ext.getCmp(activeTab.id).title);

                    //如果tab已存在，則跳到該tab
                    var target = tp.child('[title=' + '供應商-' + param + ']');
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
                            title: '供應商-' + param,
                            id: 'tab' + param,
                            itemId: "tab" + param,
                            html: '<iframe src="' + src + '" id="frame' + param + '" width="100%" height="100%" frameborder="0"></iframe>',
                            closable: true
                        };
                        var newTab = tp.add(tabItem);
                        tp.on('tabchange', function (me, newCard, oldCard) {
                            if (newCard.id == activeTab.id) {
                                T1Store.load();
                            }
                        });

                        tp.setActiveTab(newTab);
                    }
                }
            },
            failure: function (response) {
                Ext.MessageBox.alert('錯誤', '發生例外錯誤');
            }
        });
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
            itemId: 't1Grid',
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [T1Grid]
        }]
    });

    msglabel("");
    T1Load();

});